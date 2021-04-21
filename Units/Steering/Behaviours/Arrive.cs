using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR) 
using DebugTools;
#endif
namespace Steering
{
    public class Arrive : Behaviour
    {
        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            // Calculate the distance using the target offset
            Vector3 targetOffset = context.m_positionTarget - context.m_position;
            float distance = Vector3.Distance(context.m_position, targetOffset);

            if (Vector3.Distance(context.m_position, context.m_positionTarget) <= context.m_settings.m_stoppingDistance)
            {
                m_velocityDesired = Vector3.zero;
            }
            else
            {
                // Calculate ramped speed and get the lesser speed value to use for desired velocity
                float rampedSpeed = context.m_settings.m_maxDesiredVelocity * (distance / context.m_settings.m_slowingDistance);
                float clippedSpeed = Mathf.Min(rampedSpeed, context.m_settings.m_slowingDistance);
                m_velocityDesired = (clippedSpeed / distance) * targetOffset;
            }

            return m_velocityDesired - context.m_velocity;
        }

#if (UNITY_EDITOR)

        public override void OnDrawGizmos(BehaviourContext context)
        {
            GizmoDrawing.DrawCircle(context.m_positionTarget, context.m_settings.m_stoppingDistance,Color.white);
            GizmoDrawing.DrawCircle(context.m_positionTarget, context.m_settings.m_slowingDistance, Color.black);
        }
#endif
    }
}
