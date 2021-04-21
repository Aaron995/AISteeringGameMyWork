using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR)
using DebugTools;
#endif
namespace Steering
{
    public abstract class Avoid : Behaviour
    {
        protected Ray ray;
        protected Vector3 hitPoint;
        protected float rayAngle = 0f;
        protected float rayOffset = 0f;
        protected float rayScale = 1f;

        public Avoid() { }
        public Avoid(float angle, float offset, float scale)
        {
            rayAngle = angle;
            rayOffset = offset;
            rayScale = scale;
        }
        public abstract Vector3 CalculateDesiredVelocity(RaycastHit hit, BehaviourContext context);

        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            if (rayOffset == 0f && rayAngle == 0f)
            {
                ray = new Ray(context.m_position, context.m_velocity);
            }
            else
            {
                Vector3 position = context.m_position;
                if (rayOffset != 0f)
                {
                    Vector3 perpendicular = Vector3.Cross(Vector3.up, context.m_velocity).normalized;
                    position += perpendicular;
                }
                Vector3 direction = Quaternion.AngleAxis(rayAngle, Vector3.up) * context.m_velocity;
                ray = new Ray(position, direction);
            }
            // Cast the raycast and store hit info 
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, context.m_settings.m_avoidDistance * rayScale, context.m_settings.m_avoidLayerMask)
                || context.m_velocity.magnitude < 0.5f)
            {
                // Return zero steering force if raycast didn't hit
                return Vector3.zero;
            }

            // Save hit point for gizmo drawing
            hitPoint = hit.point;

            // Calculate desired velocity
            m_velocityDesired = CalculateDesiredVelocity(hit, context);

            // Make sure desired velocity and velocity are not aligned 
            float angle = Vector3.Angle(m_velocityDesired, context.m_velocity);
            if (angle > 179)
            {
                m_velocityDesired = Vector3.Cross(Vector3.up, context.m_velocity);
            }

            // Return steering force
            return m_velocityDesired - context.m_velocity;
        }

#if (UNITY_EDITOR)

        public override void OnDrawGizmos(BehaviourContext context)
        {
            GizmoDrawing.DrawRayWithDisc(ray.origin, ray.direction * (context.m_settings.m_avoidDistance * rayScale), Color.yellow);
            GizmoDrawing.DrawDot(hitPoint, Color.green);
        }
#endif
    }
}
