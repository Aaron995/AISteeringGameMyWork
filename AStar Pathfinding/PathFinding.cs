using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldGrid;

namespace AStarPathfinding
{
    using Grid = WorldGrid.Grid<PathNode>;
    public class PathFinding
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        /// <summary>
        /// Class for A* Pathfinding.
        /// </summary>
        /// <param name="_gridWidth">The width of the Grid.</param>
        /// <param name="_gridHeight">The height of the Grid.</param>
        /// <param name="_cellSize">How big each cell is.</param>
        /// <param name="_originPos">The position the grid will start creating from.</param>
        /// <param name="_obstacleLayer">The layer(s) were all the obstacles are on.</param>
        /// <param name="_monoObject">A MonoBehaviour to run Coroutines on.</param>
        public PathFinding(int _gridWidth, int _gridHeight, float _cellSize, Vector3 _originPos, LayerMask _obstacleLayer ,MonoBehaviour _monoObject)
        {
            m_requestManager = new PathRequestManager(this);
            m_grid = new Grid(_gridWidth, _gridHeight, _cellSize, _originPos,_obstacleLayer ,(int x, int y, Grid grid) => new PathNode(x, y, grid));
            monoObject = _monoObject;
        }

        /// <summary>
        /// Class for A* Pathfinding.
        /// </summary>
        /// <param name="_grid">Grid used for pathfinding.</param>
        /// <param name="_monoObject">A MonoBehaviour to run Coroutines on.</param>
        public PathFinding(Grid _grid, MonoBehaviour _monoObject)
        {
            m_requestManager = new PathRequestManager(this);
            m_grid = _grid;
            monoObject = _monoObject;
        }
        /// <summary>
        /// Class for A* Pathfinding.
        /// </summary>
        /// <param name="_settings">Grid settings to set-up a Grid for pathfinding.</param>
        /// <param name="_monoObject">A MonoBehaviour to run Coroutines on.</param>
        public PathFinding(GridSettings _settings, MonoBehaviour _monoObject)
        {
            m_requestManager = new PathRequestManager(this);
            m_grid = new Grid(_settings,(int x, int y, Grid grid) => new PathNode(x, y, grid));
            monoObject = _monoObject;
        }

        /// <summary>
        /// The request manager to handle pathfinding requests.
        /// </summary>
        public PathRequestManager m_requestManager;

        /// <summary>
        /// The grid of pathnodes using to determine a path.
        /// </summary>
        public Grid m_grid;

        // MonoBehaviour where we can run the coroutines on
        private MonoBehaviour monoObject;

        public void StartFindPath(PathNode startNode, PathNode endNode)
        {
            monoObject.StartCoroutine(FindPath(startNode, endNode));
        }       

        IEnumerator FindPath(PathNode startNode, PathNode targetNode)
        {
            // Initalize varibles used for path finding and add 
            PathNode[] nodesPath = new PathNode[0];
            bool pathSucces = false;
            Heap<PathNode> openSet = new Heap<PathNode>(m_grid.GetWidth() * m_grid.GetHeight());
            HashSet<PathNode> closedSet = new HashSet<PathNode>();

            // Add the starting node to the openset
            openSet.Add(startNode);

            if (openSet.Count == 0)
            {
                Debug.Log("No path could be made");
            }

            // Reset all nodes to default g and f costs
            foreach (PathNode node in m_grid.m_gridArray)
            {
                node.gCost = int.MaxValue;
                node.CalculateFCost();
                node.cameFromNode = null;
            }
            // Calculate g, h and f cost for starting node

            startNode.gCost = 0;
            startNode.hCost = GetDistance(startNode, targetNode);
            startNode.CalculateFCost();

            // Start looping through nodes until a path is found or it ran out of nodes to check
            while (openSet.Count > 0)
            {
                PathNode currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSucces = true;                    
                    break;
                }

                foreach (PathNode neighbourNode in GetNeighbourNodes(currentNode))
                {
                    // Checks if closed set contains neighbour node already 
                    if (closedSet.Contains(neighbourNode))
                    {
                        continue;
                    }
                    // Check if we can walk to this node
                    if (!neighbourNode.m_isWalkable)
                    {
                        closedSet.Add(neighbourNode);
                        continue;
                    }
                    
                    // Calculate gcost 
                    int tentativeGCost = currentNode.gCost + GetDistance(neighbourNode, targetNode);
                    // Check if the new calculated gcost if lower then the already known gcost
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        // Set the node where we came from
                        neighbourNode.cameFromNode = currentNode;
                        // Update g,h and f cost
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = GetDistance(neighbourNode, targetNode);
                        neighbourNode.CalculateFCost();

                        if (openSet.Contains(neighbourNode))
                        {
                            openSet.UpdateItem(neighbourNode);
                        }
                        else
                        {
                            openSet.Add(neighbourNode);
                        }
                    }

                }
            }

            yield return null;

            if (pathSucces)
            {
                nodesPath = RetracePath(startNode, targetNode);
            }

            m_requestManager.FinishedProcessingPath(nodesPath, pathSucces);
        }

        PathNode[] RetracePath(PathNode startNode, PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            PathNode currentNode = endNode;
            // Loop through nodes looking where we came from to trace our path
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.cameFromNode;
            }
            // Add the start node and reverse the path (so it goes start to finish) and return it as an array
            if (currentNode == startNode)
            {
                path.Add(currentNode);
            }
            path.Reverse();

            return path.ToArray();
        }

        public PathNode[] GetNeighbourNodes(PathNode _node)
        {
            // Initialize list for neighbouring nodes
            List<PathNode> neighbourList = new List<PathNode>();

            // Check if we can go left on the X
            if (_node.x - 1 >= 0)
            {
                // Left
                neighbourList.Add(m_grid.GetGridObject(_node.x - 1, _node.y));
                // Left up
                if (_node.y + 1 < m_grid.GetHeight())
                {
                    neighbourList.Add(m_grid.GetGridObject(_node.x - 1, _node.y + 1));
                }
                // Left down
                if (_node.y - 1 >= 0)
                {
                    neighbourList.Add(m_grid.GetGridObject(_node.x - 1, _node.y - 1));
                }
            }
            // Check if we can go right on the x
            if (_node.x + 1 < m_grid.GetWidth())
            {
                // Right
                neighbourList.Add(m_grid.GetGridObject(_node.x + 1, _node.y));
                // Right up
                if (_node.y + 1 < m_grid.GetHeight())
                {
                    neighbourList.Add(m_grid.GetGridObject(_node.x + 1, _node.y + 1));
                }
                // Right down
                if (_node.y - 1 >= 0)
                {
                    neighbourList.Add(m_grid.GetGridObject(_node.x + 1, _node.y - 1));
                }
            }

            // Up
            if (_node.y + 1 < m_grid.GetHeight())
            {
                neighbourList.Add(m_grid.GetGridObject(_node.x, _node.y + 1));
            }
            // Down
            if (_node.y - 1 >= 0)
            {
                neighbourList.Add(m_grid.GetGridObject(_node.x, _node.y - 1));
            }

            return neighbourList.ToArray();
        }

        private int GetDistance(PathNode nodeA, PathNode nodeB)
        {
            int xDistance = Mathf.Abs(nodeA.x - nodeB.x);
            int yDistance = Mathf.Abs(nodeA.y - nodeB.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

#if (UNITY_EDITOR)

        public void DebugUpdate()
        {
            m_grid.DebugUpdate();
        }
        public void OnDrawGizmos()
        {
            m_grid.OnDrawGizmos();
        }
#endif
    }

}
