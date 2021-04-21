using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR) 
using DebugTools;
#endif


namespace Steering
{
    using BehaviourList = List<IBehaviour>;
    [Serializable]
    public class Steering : MonoBehaviour
    {        
        [Header("Steering Settings")]
        public string m_label;
        public SteeringSettings m_settings; // Steering settings for all behaviours
        public float m_maxMovementSpeed;
        public Transform heightObject;
        public GameObject selectedModel;

        [Header("Steering Runtime")]
        public Vector3 m_position = Vector3.zero; // Current Position
        public Vector3 m_positionTarget; // Target position
        public Vector3 m_velocity = Vector3.zero; // Current velocity
        public Vector3 m_steering; // Steering force
        public BehaviourList m_behaviours = new BehaviourList(); // All behaviours
        public BehaviourContext behaviourContext;

            
        void Start()
        {
            m_position = transform.position;
        }


        void FixedUpdate()
        {
            if (m_settings != null)
            {
                UpdateSteering();
            }
        }

        private void UpdateSteering()
        {
            // Calculate steering force
            m_steering = Vector3.zero;
            foreach (IBehaviour behaviour in m_behaviours)
            {
                m_steering += behaviour.CalculateSteeringForce(Time.fixedDeltaTime, behaviourContext);
            }

            // Make sure steering is only done in the xz plane
            m_steering.y = 0f;

            // Clamp steering force to max and apply mass
            m_steering = Vector3.ClampMagnitude(m_steering, m_settings.m_maxSteeringForce);
            m_steering /= m_settings.m_mass;

            // Update Velocity with steering force and update position and target position            
            m_velocity = Vector3.ClampMagnitude(m_velocity + m_steering, m_maxMovementSpeed);
            m_position += m_velocity * Time.fixedDeltaTime;
            m_positionTarget = behaviourContext.m_positionTarget;

            // Update the context
            behaviourContext.m_position = m_position;
            behaviourContext.m_velocity = m_velocity;

            // Set the m_positions.y value to the returned value
            m_position.y = HeightChecker();

            // Update object with new position
            transform.position = m_position;

            // Make sure velocity is not zero to rotate
            if (m_velocity != Vector3.zero && !m_behaviours.OfType<Idle>().Any())
            {
                // Update object rotation with smooth rotation
                Vector3 tempTargetPosition = m_position + Time.fixedDeltaTime * m_velocity;
                if (tempTargetPosition != Vector3.zero)
                {
                    Vector3 dif = tempTargetPosition - m_position;
                    if (dif != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(dif);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_settings.m_rotationSpeed * Time.fixedDeltaTime);
                    }
                }
            }
        }

        private float HeightChecker()
        {
            RaycastHit hit;
            // Cast a ray to the ground and return the y value of the hit point
            if (Physics.Raycast(heightObject.position, heightObject.transform.TransformDirection(Vector3.down), out hit, 3, m_settings.layerMask))
            {
                return hit.point.y;
            }

            return m_position.y;
        }

        public void SetBehaviors(BehaviourList behaviors, string label = "")
        {
            // Create behaviour context            
            behaviourContext = new BehaviourContext(m_position, m_position, m_velocity, m_settings);

            // Remember the new settings
            m_label = label;
            m_behaviours = behaviors;

            // Start all behaviors
            foreach (IBehaviour behavior in m_behaviours)
            {
                behavior.Start(behaviourContext);
            }

        }


        //--------------------------------------------------------------------------------
        //-----------------------DEBUG STUFF----------------------------------------------
        //--------------------------------------------------------------------------------
#if (UNITY_EDITOR)

        private void OnDrawGizmos()
        {
            GizmoDrawing.DrawRayWithDisc(transform.position, m_velocity, Color.red);
            GizmoDrawing.DrawLabel(transform.position, m_label, Color.black);

            //Draw all gizomos in all behaviors
            foreach (IBehaviour behaviour in m_behaviours)
            {
                behaviour.OnDrawGizmos(behaviourContext);
            }
        }
#endif
    }
}
