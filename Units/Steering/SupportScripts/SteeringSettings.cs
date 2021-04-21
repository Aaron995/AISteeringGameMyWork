using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering
{
    [CreateAssetMenu(fileName = "Steering Settings", menuName = "Steering/Steering Settings")]

    public class SteeringSettings : ScriptableObject
    {
        [Header("Steering Settings")]
        public float m_maxMoveSpeed = 3f; // Max vehicle speed in m/s
        public float m_mass = 70f; // Mass in kg
        public float m_maxSteeringForce = 3f; // Max force in m/s
        public float m_maxDesiredVelocity = 3f; // Max desired velocity in m/s
        public float m_rotationSpeed = 3f; // Rotation speed of the vehicle

        [Header("Height Manager")]
        public LayerMask layerMask;

        [Header("Arrive")]
        public float m_arriveDistance = 1f; // Distance to target when we reach zero velocity in m
        public float m_slowingDistance = 2f; // Distance to the stop position where we start slowing down

        [Header("Seeking")]
        public float m_stoppingDistance = 1f; // Distance from target you are seeking in m

        [Header("Follow Path")]
        public bool m_loopPath = true; // Loops the path
        public bool m_allowedToReturn = false; // Walk the path backwards
        public PathStatusEnum m_startPathMode = PathStatusEnum.forwards; // Start off walking backwards or forwards
        public float m_waypointRadius = 1f; // The distance a behaviour will be able to "collect" the waypoint

        [Header("Flee")]
        public float m_fleeRange = 1f; // Distance until fleeing stops from target in m

        [Header("Pursuit and Evade")]
        public float m_lookAheadTime = 1f; // Look ahead time from target in s

        [Header("Evade")]
        public float m_evadeRange = 1f; // Distance until behaviour will stop evading the target in m

        [Header("Wander")]
        public float m_wanderCircleDistance = 5f; // Cicle distance in m
        public float m_wanderCircleRadius = 5f; // Cirlce radius in m
        public float m_wanderNoiseAngle = 10f; // Noise angle in degrees

        [Header("Obstacle Avoidance")]
        public float m_avoidMaxForce = 5f; // Max steering force to avoid obstacles in m/s
        public float m_avoidDistance = 2.5f; // Max distance to avoid objects
        public LayerMask m_avoidLayerMask; // The layer(s) the obstacles are on

        [Header("Avoid Wall")]
        public float m_avoidWallMaxForce = 5f; // Max steering force to avoid obstacles in m/s
        public float m_avoidWallDistance = 2.5f; // Max distance to avoid objects
        public LayerMask m_avoidWallLayerMask; // The layer(s) the obstacles are on

        [Header("Hide")]
        public float m_hideOffset = 1f; // The distance from surface on other side of the collider
        public LayerMask m_hideLayerMask; // The layer(s) you can hide behind

        [Header("Flocking")]
        public LayerMask m_FlockLayer; // The layers name containing all the agents in this flock (group)

        public float m_FlockAlignmentWeight = 1.0f; // The alignment weight for the agents in this flock (set to zero to ignore alignment)
        public float m_FlockCohesionWeight = 1.0f; // The cohesion weight for the agents in this flock (set to zero to ignore cohesion)
        public float m_FlockSeparationWeight = 1.5f; // The separation weight for the agents in this flock (set to zero to ignore separation)

        public float m_FlockAlignmentRadius = 6.0f; // The flocking alignment radius
        public float m_FlockCohesionRadius = 6.0f; // The flocking cohesion radius (how close the agents stick together)
        public float m_FlockSeparationRadius = 3.0f; // The flocking separation radius (the distance between agents in a flock)

        public float m_FlockVisibilityAngle = 90.0f; // The agent visibility angle
    }
}
    