using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering 
{ 
    [RequireComponent(typeof(Steering))]
    public class SimpleBrain : MonoBehaviour
    {
        //Supported Behaviors
        public enum BehaviourEnum { Keyboard, SeekClickPoint, Seek, Flee, Pursue, Evade, Wander, FollowPath, Hide,AvoidObstacle, Idle, Flock, NotSet }

        [Header("Manual")]
        public BehaviourEnum[] m_behaviors; // the requested behavior
        public GameObject m_target = null; //the target we are working with
        public Transform[] m_waypoints; //Waypoints for pathing mode

        [Header("Private")]
        private Steering m_steering;

        public SimpleBrain()
        {
            m_behaviors = null;
            m_target = null;
        }


        private void Start()
        {
            if (m_target == null)
            {
                m_target = GameObject.Find("Player");
            }

            if (m_target == null)
            {
                m_target = GameObject.FindGameObjectWithTag("Player");
            }

            if (m_target == null)
            {
                m_target = GameObject.Find("Target");
            }

            //Get steering script
            m_steering = GetComponent<Steering>();

            //Configure steering
            List<IBehaviour> behaviors = new List<IBehaviour>();
            string label ="";
            foreach (BehaviourEnum behaviour in m_behaviors)
            {
                switch (behaviour)
                {
                    case BehaviourEnum.Keyboard:
                        behaviors.Add(new Keyboard(GetComponent<UnityEngine.InputSystem.PlayerInput>().actions));                    
                        break;
                    case BehaviourEnum.SeekClickPoint:
                        behaviors.Add(new PointAndClickSeek(GetComponent<UnityEngine.InputSystem.PlayerInput>().actions));                        break;
                    case BehaviourEnum.Seek:
                        behaviors.Add(new SeekMovement(m_target));                    
                        break;
                    case BehaviourEnum.FollowPath:
                        behaviors.Add(new FollowPath(m_waypoints, m_steering.m_settings));
                        break;
                    case BehaviourEnum.Flee:
                        behaviors.Add(new Flee(m_target));
                        break;
                    case BehaviourEnum.Pursue:
                        behaviors.Add(new Pursue(m_target));
                        break;
                    case BehaviourEnum.Evade:
                        behaviors.Add(new Evade(m_target));
                        break;
                    case BehaviourEnum.Wander:
                        behaviors.Add(new Wander(gameObject.transform));
                        break;
                    case BehaviourEnum.AvoidObstacle:
                        behaviors.Add(new AvoidObstacle());
                        break;
                    case BehaviourEnum.Hide:
                        behaviors.Add(new Hide(m_target));
                        break;
                    case BehaviourEnum.Idle:
                        behaviors.Add(new Idle());
                        break;
                    case BehaviourEnum.Flock:
                        behaviors.Add(new Flock(GetComponent<Collider>()));
                        break;
                    default:
                        Debug.LogError($"Behavior of type {behaviour} not implemented yet!");
                        break;
                }
                label = label + ", " + behaviour.ToString();
            }

            m_steering.SetBehaviors(behaviors, label);

        }

      
    }
}