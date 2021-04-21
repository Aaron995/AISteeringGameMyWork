using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Steering
{
    public class Keyboard : Behaviour
    {
        // The input action to move
        public InputAction move;
        public Keyboard(InputActionAsset controls)
        {
            // Check if controls are present
            if (controls != null)
            {
                // Get the input action for moving and subscribe with our move event to it.
                move = controls.FindActionMap("Player").FindAction("Move");
                if (move == null)
                {
                    Debug.LogError("Cannot find move action!");
                    return;
                }
                move.performed += Moving;
                move.canceled += StoppedMoving;                
            }
            else
            { 
                Debug.LogError("No controls found!");
            }
        }

        private Vector3 moveVector;
        public void Moving(InputAction.CallbackContext context)
        {
            Vector2 inputVect = context.ReadValue<Vector2>(); // Get the movement vector
            moveVector = new Vector3(inputVect.x, 0, inputVect.y); // Convert it to a vector 3
        }

        public void StoppedMoving(InputAction.CallbackContext context)
        {
            moveVector = Vector3.zero;
        }
        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            // Update target position 
            if (moveVector != Vector3.zero)
            {
                context.m_positionTarget = context.m_position + moveVector.normalized * context.m_settings.m_maxDesiredVelocity;
            }
            else
            {
                context.m_positionTarget = context.m_position;
            }

            // Calculate desired velocity and return steering force
            m_velocityDesired = (context.m_positionTarget - context.m_position).normalized * context.m_settings.m_maxDesiredVelocity;
            return m_velocityDesired - context.m_velocity;
        }





    }
}
