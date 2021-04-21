using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if (UNITY_EDITOR) 
using DebugTools;
#endif

namespace Steering
{
    public class PointAndClickSeek : Behaviour
    {
        public InputAction clickMove;
        private Vector3 targetPosHold = Vector3.zero;
        public override void Start(BehaviourContext context)
        {
            base.Start(context);            
        }

        public PointAndClickSeek(InputActionAsset controls)
        {
            if (controls != null)
            {
                // Get the input action for moving and subscribe with our move event to it.
                clickMove = controls.FindActionMap("Player").FindAction("ClickMove");
                if (clickMove == null)
                {
                    Debug.LogError("Cannot find ClickMove action!");
                    return;
                }

                clickMove.performed += MoveToClick;
            }
            else
            {
                Debug.LogError("No controls found!");
            }
        }

        public void MoveToClick(InputAction.CallbackContext context)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit, 100))
            {
                targetPosHold = hit.point;                
            }
        }
        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            if (targetPosHold == Vector3.zero)
            {
                targetPosHold = context.m_position;
            }
            // Set y to current y for distance calc
            targetPosHold.y = context.m_position.y;
            if (targetPosHold != context.m_positionTarget)
            {
                context.m_positionTarget = targetPosHold;
            }


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
#if (UNITY_EDITOR)

        public override void OnDrawGizmos(BehaviourContext context)
        {
            base.OnDrawGizmos(context);
            GizmoDrawing.DrawDot(context.m_positionTarget, Color.white);
        }
#endif
    }
}