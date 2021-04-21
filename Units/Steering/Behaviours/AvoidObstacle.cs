using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR) 
using DebugTools;
#endif

namespace Steering
{
    public class AvoidObstacle : Avoid
    {
        public AvoidObstacle() { }
        public AvoidObstacle(float angle, float offset, float scale) 
        {
            rayAngle = angle;
            rayOffset = offset;
            rayScale = scale;
        }
        public override Vector3 CalculateDesiredVelocity(RaycastHit hit, BehaviourContext context)
        {
            return (hit.point - hit.collider.transform.position).normalized * context.m_settings.m_avoidMaxForce;
        }        

        public override void OnDrawGizmos(BehaviourContext context)
        {
            base.OnDrawGizmos(context);
        }
    }
}
