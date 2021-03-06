using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR) 
using DebugTools;
#endif

namespace Steering
{
    public class Pursue : Behaviour
    {
        private GameObject target;
        private Vector3 targetPos;
        public Pursue(GameObject _target)
        {
            target = _target;
            targetPos = _target.transform.position;
        }

        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {   
            // Get the last target position and update the current one
            Vector3 prevTargetPos = targetPos;
            targetPos = target.transform.position;

            // Calculate target speed
            Vector3 targetVelocity = (targetPos - prevTargetPos) / dt;

            // Calculate target position 
            context.m_positionTarget = targetPos + targetVelocity * context.m_settings.m_lookAheadTime;

            // Calculate deisred velocity
            if (ArriveEnabled(context) && WithinArriveSlowingDistnace(context))
            {
                m_velocityDesired = CalculateArriveSteeringForce(dt, context);
            }
            else
            {
                m_velocityDesired = (context.m_positionTarget - context.m_position).normalized * context.m_settings.m_maxDesiredVelocity;
            }

            // Return steering force
            return m_velocityDesired - context.m_velocity;
        }
#if (UNITY_EDITOR)

        public override void OnDrawGizmos(BehaviourContext context)
        {
            base.OnDrawGizmos(context);
            GizmoDrawing.DrawDot(context.m_positionTarget, Color.black);
        }
#endif
    }
}
