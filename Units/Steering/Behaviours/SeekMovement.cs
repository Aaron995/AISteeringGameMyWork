using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{
    public class SeekMovement : Behaviour
    {
        private GameObject target;
        private Vector3 targetPos;

        public SeekMovement(GameObject _target)
        {
            target = _target;
        }
        
        public SeekMovement(Vector3 _targetPosition)
        {
            targetPos = _targetPosition;
        }
        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            // Get target position 
            if (target == null)
            {
                context.m_positionTarget = targetPos;
            }
            else
            {
                context.m_positionTarget = target.transform.position;
            }
            // Set the position target y to current one
            context.m_positionTarget.y = context.m_position.y;

            // Calculate desired velocity and return steering force
            if (ArriveEnabled(context) && WithinArriveSlowingDistnace(context))
            {
                m_velocityDesired = CalculateArriveSteeringForce(dt, context);
            }
            else
            {
                m_velocityDesired = (context.m_positionTarget - context.m_position).normalized * context.m_settings.m_maxDesiredVelocity;                  
            }
            return m_velocityDesired - context.m_velocity;
        }
    }
}
