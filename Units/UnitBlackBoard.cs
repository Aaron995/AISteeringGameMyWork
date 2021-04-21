using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Units;
using System;

namespace BehaviourTree
{
    /// <summary>
    /// Blackboard version used for the unit behaviour tree.
    /// </summary>
    [Serializable]
    public class UnitBlackBoard : BlackBoard
    {
        /// <summary>
        /// The stats of the unit.
        /// </summary>
        public UnitStats unitStats;
        /// <summary>
        /// All enemies in vision range.
        /// </summary>
        public GameObject[] enemiesInRange;
        /// <summary>
        /// All allies in vision range.
        /// </summary>
        public GameObject[] alliesInRange; 
        /// <summary>
        /// Destination of the unit.
        /// </summary>
        public Vector3 destination; 
        /// <summary>
        /// Indicates if the unit needs a new path assigned.
        /// </summary>
        public bool newPathNeeded; 
        /// <summary>
        /// A Check to see if the path is already generated or still busy.
        /// </summary>
        public bool generatingPath; 
        /// <summary>
        /// The current path the unit is walking.
        /// </summary>
        public Vector3[] path;
        /// <summary>
        /// Used to check if the unit already visited the upgrade location already.
        /// </summary>
        public bool upgraded; 
        /// <summary>
        /// The current index the unit was on in the path.
        /// </summary>
        public int currentPathIndex; 
        /// <summary>
        /// A check to see if the steering are been setup.
        /// </summary>
        public bool steeringSettingsSetup; 
        /// <summary>
        /// The target the unit is targeting for combat.
        /// </summary>
        public GameObject combatTarget; 
        /// <summary>
        /// Timer used to keep track of how long a unit has been chasing their target
        /// </summary>
        public float chaseTimer;
        /// <summary>
        /// Timer used to keep track of time between attack.
        /// </summary>
        public float swingTimer;
    }
}