using UnityEngine;

namespace Steering
{
    public class BehaviourContext
    {
        public Vector3 m_position; // The current position
        public Vector3 m_positionTarget; // Target position
        public Vector3 m_velocity; // The current velocity
        public SteeringSettings m_settings; // All steering settings
        public int m_pathIndex = 0;

        public BehaviourContext(Vector3 position,Vector3 targetPos, Vector3 velocity, SteeringSettings settings)
        {
            m_position = position;
            m_positionTarget = targetPos;
            m_velocity = velocity;
            m_settings = settings;
        }
    }
}