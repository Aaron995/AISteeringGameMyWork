using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steering;

namespace Units
{
    [CreateAssetMenu(fileName = "Unit Settings", menuName = "Units/Unit Settings")]
    public class UnitSettings : ScriptableObject
    {
        /// <summary>
        /// Steering setting used for guard path A.
        /// </summary>
        public SteeringSettings m_guardPathASteeringSettings;
        /// <summary>
        /// Steering setting used for guard path B.
        /// </summary>
        public SteeringSettings m_guardPathBSteeringSettings;
        /// <summary>
        /// Steering setting used for all other behaviours.
        /// </summary>
        public SteeringSettings m_otherSteeringSettings;
        /// <summary>
        /// The Attack range of the units in meters.
        /// </summary>
        public float m_attackRange; 
        /// <summary>
        /// Time in between each behaviour tree update, in seconds.
        /// </summary>
        public float m_behaviourTreeUpdateTime; 
    }
}
