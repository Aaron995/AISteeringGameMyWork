using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Steering;
using AStarPathfinding;
using BehaviourTree;
using Units.Manager;

namespace Units
{
    [RequireComponent(typeof(Steering.Steering))] // Using steering class for movement 
    [RequireComponent(typeof(UnitStatus))]// Using UnitStatus for storing information and interfaces to keep the brain script clean
    public class UnitBrain : Brain
    {
        // Setting for the unit, this includes steering settings, attack range and behaviour tree update delay
        [SerializeField] private UnitSettings m_settings; 
        // Reference to the status script to access its information
        private UnitStatus status;
        // Reference to the steering script to various things related to the units movement
        private Steering.Steering steering;
        // The root node for the behaviour tree
        private RootNode rootNode;
        // The black board used for the behaviour tree
        [SerializeField] private UnitBlackBoard blackBoard;
        // Bool to stop running the information in the update.
        private bool runUpdate = false;
        // The collider on the unit
        private Collider unitCollider;
        // Timer used for the delay on the behaviour tree update
        private float behaviourTreeUpdateTimer;     
        
        private void Awake()
        {
            // Gather all references needed
            status = GetComponent<UnitStatus>();
            steering = GetComponent<Steering.Steering>();
            unitCollider = GetComponent<Collider>();
            rootNode = BuildBehaviourTree();
            // Initialize the black board
            blackBoard = new UnitBlackBoard();
            // Sub to multiple events around the game
            status.onInitialization.AddListener(SetMaxMoveSpeed);
            status.onInitialization.AddListener(ResetSteering);
            status.onInitialization.AddListener(SetBlackBoard);
            status.onDeconstruct.AddListener(OnDeath);
            GameManager.Instance.onEndGame.AddListener(GoIdle);
        }

        /// <summary>
        /// Method used for pathfinding to return after the pathfinding request has finished.
        /// </summary>
        public void ReturnPath(PathNode[] _path, bool _success, PathRequest pathRequest)
        {
            // Check if path was a success
            if (_success)
            {
                // Convert the pathnodes to a vector3 list
                List<Vector3> pathList = new List<Vector3>();
                foreach (PathNode node in _path)
                {
                    pathList.Add(node.GetMiddleWorldPosition());
                }
                // Update our black board variables 
                blackBoard.generatingPath = false;
                blackBoard.newPathNeeded = false;
                blackBoard.path = pathList.ToArray();                
            }
            else
            {
                // Update that we stopped generating a path
                blackBoard.generatingPath = false;
            }
        }

        
        public void Update()
        {
            // Check if we are allowed to run update this cycle
            if (runUpdate)
            {
                // Call our func to update information on the black board
                UpdateBlackBoard();
                // Add to our timer and check if we are allowed to run the next update cycle for our behaviour tree
                behaviourTreeUpdateTimer += Time.deltaTime;
                if (behaviourTreeUpdateTimer > m_settings.m_behaviourTreeUpdateTime)
                {
                    if (rootNode.Update(blackBoard) == NodeStates.SUCCESS)
                    {
                        // Reset the timer and the tree if it was a success
                        rootNode.RestartTree();
                        behaviourTreeUpdateTimer = 0;
                    }
                }
            }

        }


        private RootNode BuildBehaviourTree()
        {
            return new RootNode
                // Start selector does AI behaviour when no pathfinding or destination finding is needed
                (new Selector(
                    new List<Node>()
                    {
                        // Destination and pathfinding selector
                        new Selector(
                            new List<Node>()
                            {
                                // Check if we are still busy generating a path, returns running if we are.
                                new Action(CheckIfPathIsGenerating),
                                new Selector(new List<Node>()
                                {
                                    // If the unit is able to upgrade set destination there
                                    new Action(SetDestinationToUpgrade),
                                    // If units need to heal go the healing fountain
                                    new Action(SetDestinationToHealingFountain),
                                    // Check for guard path 
                                    new Action(SetDestinationToGuardPath),
                                    // Any other behaviours
                                    new Action(SetDestinationToEnemyCastle)
                                }),
                                // Check if steering is setup
                                new Action(SetSteeringBehaviour)
                            }),
                        // Selector reacting to things happening around the unit
                        new Selector(
                            new List<Node>()
                            {
                                // Sequence to check for fleeing, then flee and check when to end it
                                new Sequence(
                                    new List<Node>()
                                    {
                                        new Action(CheckForFleeing),
                                        new Action(Flee),
                                        new Action(EndFlee)
                                    }),
                                // Sequence to check for combat, then move to their target and then attack
                                new Sequence(
                                    new List<Node>()
                                    {
                                        // Check if unit can enter combat
                                        new Action(CheckForCombat),
                                        // Move to the target if not close enough
                                        new Action(MoveToTarget),
                                        // Attack target once in range
                                        new Action(AttackTarget)
                                    }),
                                new Selector(
                                    new List<Node>()
                                    {
                                        // Check if the healing fountain is reached and wait until unit is fully healed
                                        new Action(ReachedHealingFountain),
                                        // Check if the unit reached the upgrade and already got upgraded
                                        new Action(ReachedUpgrade),
                                        // Check for loyal looking for allies after chasing
                                        new Action(CheckForAllies)
                                    }),
                                // If nothing has to be done always return success to reset the tree
                                new Action(ReturnSuccess)
                            })
                    })
                );
        }

        public NodeStates CheckIfPathIsGenerating(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // Returns running if still busy generating our path otherwise return failure
            if (myBB.generatingPath)
            {
                return NodeStates.RUNNING;
            }
            else
            {
                return NodeStates.FAILURE;
            }
        }

        public NodeStates ReturnSuccess(BlackBoard bb)
        {
            // Used to always return success
            return NodeStates.SUCCESS;
        }

        public NodeStates SetDestinationToGuardPath(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            
            // Check for all the right behaviours and settings
            if ((myBB.unitStats.m_behaviour == UnitBehaviourEnum.guard_path_a ||
                myBB.unitStats.m_behaviour == UnitBehaviourEnum.guard_path_b) && myBB.newPathNeeded &&
                status.m_unitStatus != UnitStatusEnum.fighting &&
                status.m_unitStatus != UnitStatusEnum.fleeing)
            {
                // Get the guard path from the unit manager
                blackBoard.path = UnitManager.Instance.GetGuardPath(myBB.unitStats.m_playerID, myBB.unitStats.m_behaviour);
                // Set our steering settings
                switch (myBB.unitStats.m_behaviour)
                {                   
                    case UnitBehaviourEnum.guard_path_a:
                        steering.m_settings = m_settings.m_guardPathASteeringSettings;
                        break;
                    case UnitBehaviourEnum.guard_path_b:
                        steering.m_settings = m_settings.m_guardPathBSteeringSettings;
                        break;
                    default:
                        // There should be no way that a non guard path behaviour can call this switch but just in case
                        Debug.LogError("Non guard path behaviour tried accessing a guard path!");
                        break;
                }
                // Get the closest waypoint of the guard path to start patrolling at that point.
                blackBoard.currentPathIndex = GetClosestWayPointIndex(blackBoard.path);
                // Initalize our steering behaviours and set them.
                List<IBehaviour> behaviours = new List<IBehaviour>()
                {
                    new Flock(unitCollider),
                    new FollowPath(blackBoard.path,steering.m_settings,blackBoard.currentPathIndex)
                };
                behaviours.AddRange(AddAvoidWalls());
                behaviours.AddRange(AddAvoidObstacles());
                steering.SetBehaviors(behaviours);
                // Set our status and update black board variables
                status.m_unitStatus = UnitStatusEnum.patroling;
                blackBoard.steeringSettingsSetup = true;
                blackBoard.newPathNeeded = false;
                return NodeStates.SUCCESS;
            }
            else
            {
                return NodeStates.FAILURE;
            }
        }
  
        public NodeStates SetDestinationToUpgrade(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;

            // Check for right behaviours and black board settings
            // Also checks if the avarage of our 2 attacking stats are less then 20 other wise the unit ignores
            // Upgrading and this function will return failure
            if (myBB.unitStats.m_behaviour != UnitBehaviourEnum.guard_path_a && myBB.unitStats.m_behaviour != UnitBehaviourEnum.guard_path_b
                && myBB.newPathNeeded && !myBB.upgraded && status.m_unitStatus != UnitStatusEnum.fighting &&
                status.m_unitStatus != UnitStatusEnum.fleeing &&
                (myBB.unitStats.m_attackDamage + myBB.unitStats.m_attackSpeed) / 2 < 20)
            {
                // Get the destination from the unitmanager
                blackBoard.destination = UnitManager.Instance.m_weaponUpgrade;
                // Check if our behaviour isn't wander since that goes randomly
                if (myBB.unitStats.m_behaviour != UnitBehaviourEnum.wanderer)
                {
                    // Requests our path from the unitmanager and update our black board before the request
                    blackBoard.generatingPath = true;
                    UnitManager.Instance.GetPath(ReturnPath, transform.position, myBB.destination);
                }
                else
                {
                    // Set our blackboard variable since wanderer doesn't need a path
                    blackBoard.newPathNeeded = false;
                }
                // Update the fact that we need our steering behaviours and return success
                blackBoard.steeringSettingsSetup = false;
                return NodeStates.SUCCESS;
            }
            else
            {
                return NodeStates.FAILURE;
            }
        }

        public NodeStates SetDestinationToHealingFountain(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // Check for right behaviours and black board settings
            // Also checks if the defense of our unit is lower then 20, if it isn't the unit will ignore the healing fountain
            if (myBB.unitStats.m_behaviour != UnitBehaviourEnum.guard_path_a && myBB.unitStats.m_behaviour != UnitBehaviourEnum.guard_path_b
                && myBB.newPathNeeded && !myBB.upgraded && status.m_unitStatus != UnitStatusEnum.fighting &&
                status.m_unitStatus != UnitStatusEnum.fleeing &&
                myBB.unitStats.m_defense < 20)
            {
                // Get the destination from the unitmanager
                blackBoard.destination = UnitManager.Instance.m_healingFountain;
                // Check if our behaviour isn't wander since that goes randomly
                if (myBB.unitStats.m_behaviour != UnitBehaviourEnum.wanderer)
                {
                    // Requests our path from the unitmanager and update our black board before the request
                    blackBoard.generatingPath = true;
                    UnitManager.Instance.GetPath(ReturnPath, transform.position, myBB.destination);
                }
                else
                {
                    // Set our blackboard variable since wanderer doesn't need a path
                    blackBoard.newPathNeeded = false;
                }
                // Update the fact that we need our steering behaviours and return success
                blackBoard.steeringSettingsSetup = false;
                return NodeStates.SUCCESS;
            }
            else
            {
                return NodeStates.FAILURE;
            }
        } 

        public NodeStates SetDestinationToEnemyCastle(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;

            // Check for right behaviours and black board settings
            if (myBB.unitStats.m_behaviour != UnitBehaviourEnum.guard_path_a && myBB.unitStats.m_behaviour != UnitBehaviourEnum.guard_path_b
               && myBB.newPathNeeded && status.m_unitStatus != UnitStatusEnum.fighting &&
               status.m_unitStatus != UnitStatusEnum.fleeing)
            {
                // Get the destination from the unitmanager
                blackBoard.destination = UnitManager.Instance.GetRandomEnemyCastleLocation(myBB.unitStats.m_playerID);

                // Check if our behaviour isn't wander since that goes randomly
                if (myBB.unitStats.m_behaviour != UnitBehaviourEnum.wanderer)
                {
                    // Requests our path from the unitmanager and update our black board before the request
                    blackBoard.generatingPath = true;
                    UnitManager.Instance.GetPath(ReturnPath, transform.position, blackBoard.destination);
                }
                else
                {
                    // Set our blackboard variable since wanderer doesn't need a path
                    blackBoard.newPathNeeded = false;
                }
                // Update the fact that we need our steering behaviours and return success
                myBB.steeringSettingsSetup = false;
                return NodeStates.SUCCESS;
            }
            else
            {
                return NodeStates.FAILURE;
            }
        }

        public NodeStates SetSteeringBehaviour(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;

            // Check that we have a path or that we are wanders and the steering isn't setup yet.
            if (!myBB.steeringSettingsSetup && (myBB.path != null || myBB.unitStats.m_behaviour == UnitBehaviourEnum.wanderer))
            {
                // Set our steering settings 
                steering.m_settings = m_settings.m_otherSteeringSettings;
                // Initalize the list and add flock to it.
                List<IBehaviour> behaviours = new List<IBehaviour>()
                {
                    new Flock(unitCollider),                    
                };
                // Add our avoid wall an obstacles behaviours
                behaviours.AddRange(AddAvoidWalls());
                behaviours.AddRange(AddAvoidObstacles());

                // Check for wanderer
                if (myBB.unitStats.m_behaviour == UnitBehaviourEnum.wanderer)
                {
                    // Add wander facing the destination and a seek to make sure we keep on steering towards the destination
                    behaviours.Add(new Wander(myBB.destination,gameObject.transform));
                    behaviours.Add(new SeekMovement(myBB.destination));
                }
                else
                {
                    // Add the follow path behaviour to allow the unit to navigate their path
                    behaviours.Add(new FollowPath(myBB.path, m_settings.m_otherSteeringSettings));
                }
                // Set behaviours, update the black board and set our status
                steering.SetBehaviors(behaviours);
                blackBoard.steeringSettingsSetup = true;
                status.m_unitStatus = UnitStatusEnum.traveling;
                return NodeStates.SUCCESS;
            }
            else
            {
                return NodeStates.FAILURE;
            }
        }

        public NodeStates CheckForFleeing(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // Always return failure if unit behaviour is on aggressive
            if (myBB.unitStats.m_behaviour == UnitBehaviourEnum.aggressive)
            {
                return NodeStates.FAILURE;
            }
            // If the unit defense is belowe 20 and there are enemies in range flee
            else if (myBB.unitStats.m_defense < 20 && myBB.enemiesInRange.Length > 0)                
            {
                return NodeStates.SUCCESS;
            }
            // If there are 3 times as many enemies as allies flee
            else if (myBB.enemiesInRange.Length > myBB.alliesInRange.Length * 3)
            {
                // Additional check as the calculation doesn't work when there are no allies in range
                if (myBB.alliesInRange.Length == 0 && myBB.enemiesInRange.Length < 3)
                {
                    return NodeStates.FAILURE;
                }
                else
                {
                    return NodeStates.SUCCESS;  
                }
            }
            else
            {
                return NodeStates.FAILURE;
            }
        }

        public NodeStates Flee(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // If unit is already fleeing continue fleeing
            if (status.m_unitStatus == UnitStatusEnum.fleeing)
            {
                return NodeStates.SUCCESS;
            }
            else
            {
                // Set our status to fleeing
                status.m_unitStatus = UnitStatusEnum.fleeing;
                // Set up flee behaviour with avoidance behaviours and then set behaviours
                List<IBehaviour> behaviours = new List<IBehaviour>()
                {
                    new Flee(GetClosestObject(myBB.enemiesInRange)),
                };
                behaviours.AddRange(AddAvoidWalls());
                behaviours.AddRange(AddAvoidObstacles());
                steering.SetBehaviors(behaviours);                
                return NodeStates.SUCCESS;
            }
        }

        public NodeStates EndFlee(BlackBoard bb)    
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // Check if the unit is fleeing
            if (status.m_unitStatus == UnitStatusEnum.fleeing)
            {
                // Check if there are still enemies in range
                if (myBB.enemiesInRange.Length > 0)
                {
                    // Check if the closest enemy in range is futher then our flee range
                    if (Vector3.Distance(transform.position, GetClosestObject(myBB.enemiesInRange).transform.position) > steering.m_settings.m_fleeRange)
                    {
                        // Request a new path and set the unit to idle while requesting path
                        blackBoard.newPathNeeded = true;
                        status.m_unitStatus = UnitStatusEnum.thinking;
                        steering.SetBehaviors(new List<IBehaviour>()
                        {
                            new Idle()
                        });
                        return NodeStates.SUCCESS;
                    }
                    else
                    {
                        // Unit is stil busy fleeing
                        return NodeStates.RUNNING;
                    }
                }
                else
                {
                    // There are no more enemies in sight range so the unit isn't able to see them anymore
                    // Request a new path and set the unit to idle while requesting path    
                    blackBoard.newPathNeeded = true;
                    status.m_unitStatus = UnitStatusEnum.thinking;
                    steering.SetBehaviors(new List<IBehaviour>()
                        {
                            new Idle()
                        });
                    return NodeStates.SUCCESS;
                }
            }
            else
            {
                return NodeStates.FAILURE;
            }
        }

        public NodeStates CheckForCombat(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // Check if we are close to the enemy castle and it is our destination
            if (Vector3.Distance(transform.position, myBB.destination) < 15f &&
                UnitManager.Instance.IsTargetEnemyCastle(myBB.destination,myBB.unitStats.m_playerID))
            {
                // Set our combat target as the castle and reset our chase timer as you cannot chase a castle
                blackBoard.combatTarget = UnitManager.Instance.GetCastleObject(myBB.destination);
                blackBoard.chaseTimer = 0;
                return NodeStates.SUCCESS;
            }
            // If there are no enemies in range and we don't have a target already
            else if (myBB.enemiesInRange.Length > 0 && myBB.combatTarget == null)
            {
                // Get closest target within line of sight
                GameObject closestTarget = null;
                float closestDistance = float.MaxValue;                    
                for (int i = 0; i < myBB.enemiesInRange.Length; i++)
                {
                    float distance = Vector3.Distance(transform.position, myBB.enemiesInRange[i].transform.position);
                    RaycastHit hit;
                    // If we hit the raycast hit means we hit an obstacle, so we skip that enemy since we don't have line of sight.
                    if (!Physics.Raycast(transform.position, myBB.enemiesInRange[i].transform.position, out hit, distance, steering.m_settings.m_avoidLayerMask)
                        && distance < closestDistance)
                    {
                        // If the target is fleeing and the unit isn't aggressive we don't wanna chase the target.
                        if (myBB.unitStats.m_behaviour != UnitBehaviourEnum.aggressive && 
                            myBB.enemiesInRange[i].GetComponent<IDamageable>().m_unitStatus == UnitStatusEnum.fleeing)
                        {
                            continue;
                        }
                        closestTarget = myBB.enemiesInRange[i];
                        closestDistance = distance;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (closestTarget == null)
                {
                    // If no target found we reset our chase timer and make sure our target is reset to null
                    blackBoard.combatTarget = null;
                    blackBoard.chaseTimer = 0;
                    return NodeStates.FAILURE;
                }
                else
                {
                    // Update our target
                    blackBoard.combatTarget = closestTarget;
                    return NodeStates.SUCCESS;
                }
                
                
            }
            else if (myBB.combatTarget != null)
            {
                // If we already have a target conintue combat with that target
                return NodeStates.SUCCESS;
            }
            else
            {
                return NodeStates.FAILURE;
            }
        }

        public NodeStates MoveToTarget(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // Check if our combat target is the enemy castle
            if (UnitManager.Instance.IsTargetEnemyCastle(myBB.combatTarget.transform.position, myBB.unitStats.m_playerID))
            {
                // Check if we are in range of the castle, we gain a little bit extra leeway on our attack range on the castle
                if (Vector3.Distance(transform.position, myBB.destination) < m_settings.m_attackRange + 0.5f)
                {
                    // Make sure we are idle but stil avoiding and flocking
                    List<IBehaviour> behaviours = new List<IBehaviour>()
                    {
                    new Idle(),
                    new Flock(unitCollider)
                    };
                    behaviours.AddRange(AddAvoidWalls());
                    behaviours.AddRange(AddAvoidObstacles());
                    steering.SetBehaviors(behaviours);
                    return NodeStates.SUCCESS;
                }
                else
                {
                    // Move towards the castle when out of range
                    List<IBehaviour> behaviours = new List<IBehaviour>()
                    {
                    new SeekMovement(myBB.destination),
                    new Flock(unitCollider)
                    };
                    behaviours.AddRange(AddAvoidWalls());
                    behaviours.AddRange(AddAvoidObstacles());
                    steering.SetBehaviors(behaviours);
                    return NodeStates.RUNNING;
                }
            }
            else
            {
                // Set our movement behaviours to move towards our target
                List<IBehaviour> behaviours = new List<IBehaviour>()
                {
                    new SeekMovement(myBB.combatTarget),
                };
                behaviours.AddRange(AddAvoidWalls());
                behaviours.AddRange(AddAvoidObstacles());
                steering.SetBehaviors(behaviours);

                // Check if our target is fleeing
                if (myBB.combatTarget.GetComponent<IDamageable>().m_unitStatus == UnitStatusEnum.fleeing)
                {               
                    // Start chasing our enemy if hes fleeing
                    status.m_unitStatus = UnitStatusEnum.chasing;               
                    // If our chase timer is over 2s we react to it based on behaviour
                    if (myBB.chaseTimer > 2)
                    {                        
                        switch (myBB.unitStats.m_behaviour)
                        {
                            case UnitBehaviourEnum.defensive:
                                // Defensive behaviour goes back to their last checkpoint saved in their path
                                // So set movement behaviours 
                                behaviours = new List<IBehaviour>()
                                {
                                    new FollowPath(myBB.path, steering.m_settings,myBB.currentPathIndex),
                                    new Flock(unitCollider)
                                };
                                behaviours.AddRange(AddAvoidWalls());
                                behaviours.AddRange(AddAvoidObstacles());
                                steering.SetBehaviors(behaviours);
                                // Update our black board since we are no longer attacking anybody and we go back to traveling
                                blackBoard.combatTarget = null;
                                status.m_unitStatus = UnitStatusEnum.traveling;
                                return NodeStates.FAILURE;
                            case UnitBehaviourEnum.loyal:
                                // Loyal behaviours tries to go to allies 
                                if (myBB.alliesInRange.Length > 0)
                                {
                                    behaviours = new List<IBehaviour>()
                                    {
                                        new SeekMovement(GetClosestObject(myBB.alliesInRange)),
                                        new Flock(unitCollider)
                                    };
                                    behaviours.AddRange(AddAvoidWalls());
                                    behaviours.AddRange(AddAvoidObstacles());
                                    steering.SetBehaviors(behaviours);
                                    blackBoard.combatTarget = null;
                                    status.m_unitStatus = UnitStatusEnum.going_to_allies;
                                }
                                // If there are none in range it goes back to its castle until they find some friendly units
                                else
                                {   
                                    blackBoard.destination = UnitManager.Instance.GetFriendlyCastleLocation(myBB.unitStats.m_playerID);
                                
                                    behaviours = new List<IBehaviour>()
                                    {
                                        new Wander(myBB.destination,transform),                                   
                                        new Flock(unitCollider)
                                    };

                                    behaviours.AddRange(AddAvoidWalls());
                                    behaviours.AddRange(AddAvoidObstacles());
                                    steering.SetBehaviors(behaviours);

                                    status.m_unitStatus = UnitStatusEnum.looking_for_allies;
                                }
                                return NodeStates.FAILURE;                            
                            case UnitBehaviourEnum.wanderer:
                                // Reset the wanderer to rethink its steps
                                steering.SetBehaviors(new List<IBehaviour>()
                                {
                                    new Idle()
                                });

                                blackBoard.newPathNeeded = true;

                                status.m_unitStatus = UnitStatusEnum.thinking;
                                return NodeStates.FAILURE;                            
                            case UnitBehaviourEnum.guard_path_a:
                                // Back to guarding their path
                                behaviours =  new List<IBehaviour>()
                                {
                                    new FollowPath(myBB.path,steering.m_settings,myBB.currentPathIndex),
                                    new Flock(unitCollider)
                                };

                                behaviours.AddRange(AddAvoidWalls());
                                behaviours.AddRange(AddAvoidObstacles());
                                steering.SetBehaviors(behaviours);

                                status.m_unitStatus = UnitStatusEnum.patroling;
                                return NodeStates.FAILURE; 
                            case UnitBehaviourEnum.guard_path_b:
                                // Back to guarding their path
                                behaviours = new List<IBehaviour>()
                                {
                                    new FollowPath(myBB.path,steering.m_settings,myBB.currentPathIndex),
                                    new Flock(unitCollider)
                                };

                                behaviours.AddRange(AddAvoidWalls());
                                behaviours.AddRange(AddAvoidObstacles());
                                steering.SetBehaviors(behaviours);
                                status.m_unitStatus = UnitStatusEnum.patroling;
                                return NodeStates.FAILURE; 
                            default:
                                // Any missing behaviours do notting with chasing an enemy and keeps on going
                                break;
                        }
                    }
                }
                else
                {
                    // Reset our chase timer if we aren't chasing our target
                    blackBoard.chaseTimer = 0;
                }


                // Check if we are in range but still move too quick
                if (Vector3.Distance(transform.position, myBB.combatTarget.transform.position) < m_settings.m_attackRange &&
                    steering.m_velocity.magnitude > 2f)
                {
                    // Reset swing timer if still moving to quickly
                    blackBoard.swingTimer = 0f;
                    return NodeStates.RUNNING;
                }
                else if (Vector3.Distance(transform.position, myBB.combatTarget.transform.position) < m_settings.m_attackRange &&
                        steering.m_velocity.magnitude <= 1.5f)
                {
                    // Stop moving if we are slowed down and start attacking
                    behaviours = new List<IBehaviour>()
                    {
                    new Idle(),
                    new Flock(unitCollider)
                    };
                    behaviours.AddRange(AddAvoidWalls());
                    behaviours.AddRange(AddAvoidObstacles());
                    steering.SetBehaviors(behaviours);
                    return NodeStates.SUCCESS;
                }
                else
                {
                    blackBoard.swingTimer = 0f;
                    return NodeStates.RUNNING;
                }
            }
        }

        public NodeStates AttackTarget(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // Set our status to fighting
            status.m_unitStatus = UnitStatusEnum.fighting;
            // Check if our swing timer if greater then our attack speed
            // The attack speed stat divded by 10 means attack per seconds
            // Meaning that 1f stands for the 1 second, so 10 attack speed means 1 attack per second.
            if (myBB.swingTimer > 1f / (myBB.unitStats.m_attackSpeed / 10))
            {
                // Damage our target based on our attack damage
                myBB.combatTarget.GetComponent<IDamageable>().TakeDamage(myBB.unitStats.m_attackDamage);
                // Invoke the attack event
                status.onAttack.Invoke();
                // Reset swing timer
                blackBoard.swingTimer = 0;
                // Check if our target died
                if (myBB.combatTarget.GetComponent<IDamageable>().m_unitStats.m_defense <= 0)
                {
                    // Clear our target if it dies
                    blackBoard.combatTarget = null;
                    // If there are no more enemies in range request a new path
                    if (myBB.enemiesInRange.Length <= 0)
                    {
                        blackBoard.newPathNeeded = true;
                    }
                }
                return NodeStates.SUCCESS;
            }
            else
            {
                return NodeStates.RUNNING;
            }
        }

        public NodeStates ReachedHealingFountain(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // Check if destination is healing fountain
            if (myBB.destination == UnitManager.Instance.m_healingFountain)
            {
                // If defense is above 100 unit will start moving away other wise make sure we are close enough
                if (myBB.unitStats.m_defense >= 100)
                {
                    blackBoard.newPathNeeded = true;
                    status.m_unitStatus = UnitStatusEnum.thinking;
                    return NodeStates.SUCCESS;
                }
                else if(Vector3.Distance(transform.position, myBB.destination) < 5f)
                {
                    steering.SetBehaviors(new List<IBehaviour>()
                    {
                        new Idle()
                    });
                    status.m_unitStatus = UnitStatusEnum.healing;
                    return NodeStates.RUNNING;
                }
                else
                {
                    return NodeStates.RUNNING;
                }
            }
            else 
            {
                return NodeStates.FAILURE;
            }
        }

        public NodeStates ReachedUpgrade(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // Check if destination is weapon upgrade
            if (myBB.destination == UnitManager.Instance.m_weaponUpgrade)
            {
                // Check if in range and wait until they get upgraded
                if (Vector3.Distance(transform.position, myBB.destination) < 5f)
                {
                    steering.SetBehaviors(new List<IBehaviour>()
                    {
                        new Idle()
                    });
                    status.m_unitStatus = UnitStatusEnum.upgrading;
                    return NodeStates.RUNNING;
                }
                // Check if already upgraded and ask for a new path
                else if (myBB.upgraded)
                {
                    blackBoard.newPathNeeded = true;
                    status.m_unitStatus = UnitStatusEnum.thinking;
                    return NodeStates.SUCCESS;
                }
                else
                {
                    return NodeStates.RUNNING;
                }
            }
            else
            {
                return NodeStates.FAILURE;
            }
        }

        public NodeStates CheckForAllies(BlackBoard bb)
        {
            UnitBlackBoard myBB = bb as UnitBlackBoard;
            // Behaviours and status are correct
            if (myBB.unitStats.m_behaviour == UnitBehaviourEnum.loyal && status.m_unitStatus == UnitStatusEnum.looking_for_allies)
            {
                // Check if any allies are in range
                if (myBB.alliesInRange.Length > 0)
                {
                    // Set black board and status
                    blackBoard.newPathNeeded = true;
                    status.m_unitStatus = UnitStatusEnum.thinking;
                }
                else
                {
                    return NodeStates.RUNNING;
                }
            }
            return NodeStates.FAILURE;


        }

        private int GetClosestWayPointIndex(Vector3[] path)
        {
            float closestDistance = Vector3.Distance(transform.position, path[0]);
            int index = 0;
            for (int i = 1; i < path.Length; i++)
            {
                float distance = Vector3.Distance(transform.position, path[i]);
                if (distance < closestDistance)
                {
                    index = i;
                    closestDistance = distance;
                }
            }

            return index;
        }

        private GameObject GetClosestObject(GameObject[] objects)
        {   
            
            GameObject closestObject = objects[0];
            float closestDistance = Vector3.Distance(transform.position, closestObject.transform.position);
            for (int i = 1; i < objects.Length; i++)
            {
                float distance = Vector3.Distance(transform.position, objects[i].transform.position);
                if (distance < closestDistance)
                {
                    closestObject = objects[i];
                }
            }
            return closestObject;
          
        }

        private void SetBlackBoard()
        {
            blackBoard.newPathNeeded = true;
            blackBoard.destination = Vector3.zero;
            blackBoard.path = null;
            blackBoard.upgraded = false;
            blackBoard.steeringSettingsSetup = false;
            blackBoard.combatTarget = null;
            blackBoard.swingTimer = status.m_unitStats.m_attackSpeed / 10;
            blackBoard.unitStats = status.m_unitStats;
            runUpdate = true;
        }

        private void UpdateBlackBoard()
        {
            blackBoard.dt = Time.deltaTime;
            blackBoard.alliesInRange = UnitManager.Instance.GetAlliesInRange(status.m_unitStats.m_sightRange, status.m_unitStats.m_playerID,gameObject);
            blackBoard.enemiesInRange = UnitManager.Instance.GetEnemiesInRange(status.m_unitStats.m_sightRange, status.m_unitStats.m_playerID, transform.position);
            if (steering.m_behaviours.OfType<FollowPath>().Any())
            {
                blackBoard.currentPathIndex = steering.behaviourContext.m_pathIndex;
            }
            if (blackBoard.combatTarget != null)
            {
                blackBoard.swingTimer += Time.deltaTime;

                if (blackBoard.combatTarget.GetComponent<IDamageable>().m_unitStatus == UnitStatusEnum.fleeing)
                {
                    blackBoard.chaseTimer += Time.deltaTime;
                }

                if (blackBoard.combatTarget.GetComponent<IDamageable>().m_unitStats.m_defense <= 0 || 
                    !blackBoard.combatTarget.activeSelf)
                {
                    blackBoard.combatTarget = null;
                }
            }
            blackBoard.upgraded = status.m_upgraded;
            blackBoard.unitStats = status.m_unitStats;
            if (blackBoard.combatTarget != null)
            {
                if (!blackBoard.combatTarget.activeSelf)
                {
                    blackBoard.combatTarget = null;
                }
            }
        }

        private void SetMaxMoveSpeed()
        {
            steering.m_maxMovementSpeed = status.m_unitStats.m_movementSpeed / 10f;
        }

        private void OnDeath(GameObject obj)
        {
            runUpdate = false;
            steering.enabled = false;
        }

        private List<IBehaviour> AddAvoidWalls()
        {
            return new List<IBehaviour>
            {
                new AvoidWall(55f,0f,0.5f),
                new AvoidWall(0f,0f,1f),
                new AvoidWall(-55f,0f,0.5f)
            };
        }

        private List<IBehaviour> AddAvoidObstacles()
        {
            return new List<IBehaviour>
            {
                new AvoidObstacle(55f,0f,0.5f),
                new AvoidObstacle(0f,0f,1f),
                new AvoidObstacle(-55f,0f,0.5f)
            };
        }

        private void GoIdle()
        {
            runUpdate = false;
            steering.SetBehaviors(new List<IBehaviour>()
            {
                new Idle()
            });
        }

        private void ResetSteering()
        {
            if (steering.behaviourContext != null)
            {
                steering.behaviourContext.m_position = transform.position;
                steering.behaviourContext.m_velocity = Vector3.zero;
            }
            steering.m_position = transform.position;
            steering.enabled = true;
        }
    }
}
