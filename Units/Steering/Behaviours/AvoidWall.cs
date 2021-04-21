using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{
    public class AvoidWall : Avoid
    {
        public AvoidWall() { }
        public AvoidWall(float angle, float offset, float scale)
        {
            rayAngle = angle;
            rayOffset = offset;
            rayScale = scale;
        }
        public override Vector3 CalculateDesiredVelocity(RaycastHit hit, BehaviourContext context)
        {
            return hit.normal * context.m_settings.m_avoidWallMaxForce;
        }        

        public override void OnDrawGizmos(BehaviourContext context)
        {
            base.OnDrawGizmos(context);
        }
    }
}