using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AStarPathfinding
{
    /// <summary>
    /// Delegate to return to after pathfinding has finished.
    /// </summary>
    /// <param name="path">The pathnode array result from the pathfinding.</param>
    /// <param name="success">Indicated if a path was found.</param>
    /// <param name="requestInfo">The orginal information of the pathfinding request.</param>
    public delegate void CallBackDelegate(PathNode[] path, bool success, PathRequest requestInfo);

    public class PathRequestManager 
    {    
        Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
        PathRequest currentPathRequest;

        bool isProcessingPath;
    
        PathFinding rootPathfinding;

        public PathRequestManager(PathFinding _rootPathfinding)
        {
            rootPathfinding = _rootPathfinding;
        }    

        /// <summary>
        /// Starts finding a path.
        /// </summary>
        /// <param name="nodeStart">The starting node.</param>
        /// <param name="nodeEnd">The target node.</param>
        /// <param name="callBackDelegate"></param>
        public void RequestPath(PathNode nodeStart, PathNode nodeEnd, CallBackDelegate callBackDelegate)
        {
            // When an entity requests a path, add it to a queue.
            PathRequest newRequest = new PathRequest(nodeStart, nodeEnd, callBackDelegate);
            pathRequestQueue.Enqueue(newRequest);
            TryProcessNext();
        }
        void TryProcessNext()
        {
            // Try to make a path from the first request in the queue
            if (!isProcessingPath && pathRequestQueue.Count > 0)
            {
                currentPathRequest = pathRequestQueue.Dequeue();
                isProcessingPath = true;
                rootPathfinding.StartFindPath(currentPathRequest.nodeStart, currentPathRequest.nodeEnd);
            }
        }
        public void FinishedProcessingPath(PathNode[] path, bool success)
        {
            currentPathRequest.callbackDelegate(path, success, currentPathRequest);
            isProcessingPath = false;
            // Try next request.
            TryProcessNext();
        }
    }

    public struct PathRequest
    {
        public PathNode nodeStart;
        public PathNode nodeEnd;
        public CallBackDelegate callbackDelegate;

        public PathRequest(PathNode _start, PathNode _end, CallBackDelegate _callbackDelegate)
        {
            nodeStart = _start;
            nodeEnd = _end;
            callbackDelegate = _callbackDelegate;
        }
    }
}
