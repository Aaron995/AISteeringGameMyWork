using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR) 
using DebugTools;
#endif
namespace Steering
{
    public enum PathStatusEnum
    {
        forwards,
        backwards,
        random
    }

    public class FollowPath : Behaviour
    {
        public int m_pathIndex { get { return pathIndex; } }
        private int pathIndex = 0;

        private Vector3[] path;
        private PathStatusEnum pathStatus;
        public FollowPath(Transform[] _path, SteeringSettings settings)
        {
            SetPath(_path);
            pathStatus = settings.m_startPathMode;            
            if (pathStatus == PathStatusEnum.backwards)
            {
                pathIndex = path.Length -1;
            }
            else if (pathStatus == PathStatusEnum.random)
            {
                pathIndex = Random.Range(0, path.Length);
            }
        }

        public FollowPath(Vector3[] _path, SteeringSettings settings)
        {
            SetPath(_path);
            pathStatus = settings.m_startPathMode;
            if (pathStatus == PathStatusEnum.backwards)
            {
                pathIndex = path.Length - 1;
            }
            else if (pathStatus == PathStatusEnum.random)
            {
                pathIndex = Random.Range(0, path.Length);
            }
        }

        public FollowPath(Vector3[] _path, SteeringSettings settings, int startingIndex)
        {
            SetPath(_path);
            pathStatus = settings.m_startPathMode;
            pathIndex = startingIndex;
        }

        public void SetPath(Transform[] _path)
        {
            List<Vector3> pathList = new List<Vector3>();
            foreach (Transform trans in _path)
            {
                pathList.Add(trans.position);
            }
            path = pathList.ToArray();
        }

        public void SetPath(Vector3[] _path)
        {
            path = _path;
        }

        public override Vector3 CalculateSteeringForce(float dt, BehaviourContext context)
        {
            if (path.Length > 0)
            {
                // Check if close enough to waypoint to continue to the next waypoint
                if (Vector3.Distance(context.m_position, path[pathIndex]) < context.m_settings.m_waypointRadius)
                {
                    NextWaypoint(context);
                }
                // Set path y to the same y as the current position
                path[pathIndex].y = context.m_position.y;

                // Set target to next waypoint
                context.m_positionTarget = path[pathIndex];

                // Calculate desired velocity and return steering force
                if (ArriveEnabled(context) && WithinArriveSlowingDistnace(context) && GoingToLastWaypoint(context))
                {
                    m_velocityDesired = CalculateArriveSteeringForce(dt, context);
                }
                else
                {
                    m_velocityDesired = (context.m_positionTarget - context.m_position).normalized * context.m_settings.m_maxDesiredVelocity;
                }
                context.m_pathIndex = pathIndex;
                return m_velocityDesired - context.m_velocity;
            }
            else
            {
                return Vector3.zero;
            }
        }


        private void NextWaypoint(BehaviourContext context)
        {
            if (pathStatus == PathStatusEnum.forwards)
            {
                if (pathIndex + 1 >= path.Length)
                {
                    if (context.m_settings.m_loopPath)
                    {
                        pathIndex = 0;
                    }
                    else if (context.m_settings.m_allowedToReturn)
                    {
                        pathStatus = PathStatusEnum.backwards;
                        pathIndex--;
                    }
                    else
                    {
                        context.m_positionTarget = context.m_position;
                    }
                }
                else
                {
                    pathIndex++;
                }
            }
            else if (pathStatus == PathStatusEnum.backwards)
            {
                if (pathIndex - 1 < 0)
                {
                    if (context.m_settings.m_loopPath)
                    {
                        pathIndex = path.Length - 1;
                    }
                    else if (context.m_settings.m_allowedToReturn)
                    {
                        pathStatus = PathStatusEnum.forwards;
                        pathIndex++;
                    }
                    else
                    {
                        context.m_positionTarget = context.m_position;
                    }
                }
                else
                {
                    pathIndex--;
                }
            }
            else if (pathStatus == PathStatusEnum.random)
            {
                int randomWaypoint = Random.Range(0, path.Length);
                while (randomWaypoint == pathIndex)
                {
                    randomWaypoint = Random.Range(0, path.Length);
                }
                pathIndex = randomWaypoint;
            }
        }
        private bool GoingToLastWaypoint(BehaviourContext context)
        {
            if (!context.m_settings.m_loopPath)
            {
                if (pathStatus == PathStatusEnum.forwards)
                {
                    if (pathIndex == path.Length - 1)
                    {
                        return true;
                    }
                }
                else if (pathStatus == PathStatusEnum.backwards)
                {
                    if (pathIndex == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
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