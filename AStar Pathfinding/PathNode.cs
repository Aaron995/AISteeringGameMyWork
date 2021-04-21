using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace AStarPathfinding
{
    // We are using our custom grid class so we set Grid as our custom class
    using Grid = WorldGrid.Grid<PathNode>;

    /// <summary>
    /// A node in the Grid used for pathfinding.
    /// </summary>
    public class PathNode : IHeapItem<PathNode>
    {
        /// <summary>
        /// The X position of the node in the Grid. 
        /// </summary>
        public int x;
        /// <summary>
        /// The Y position of the node in the Grid.
        /// </summary>
        public int y;

        /// <summary>
        /// The G cost of this node, used in A* Pathfinding.
        /// </summary>
        public int gCost;
        /// <summary>
        /// The H cost of this node, used in A* Pathfinding.
        /// </summary>
        public int hCost;
        /// <summary>
        /// The F cost of this node, used in A* Pathfinding.
        /// </summary>
        public int fCost;

        /// <summary>
        /// The index of where this node is in the heap.
        /// </summary>
        public int HeapIndex;

        /// <summary>
        /// The node we came from during A* pathfinding.
        /// </summary>
        public PathNode cameFromNode;

        /// <summary>
        /// Checks if this node is walkable for units.
        /// </summary>
        public bool m_isWalkable { private set; get; }

        /// <summary>
        /// Reference to the Grid the node is in.
        /// </summary>
        private Grid grid;

        /// <summary>
        /// A node used for A* Pathfinding.
        /// </summary>
        /// <param name="x">The X position in the Grid.</param>
        /// <param name="y">The Y position in the Grid.</param>
        /// <param name="grid">The Grid the node is in.</param>
        public PathNode(int x, int y, Grid grid)
        {
            this.x = x;
            this.y = y;
            this.grid = _grid;

#if (UNITY_EDITOR)

            grid.m_onGizmoDraw.AddListener(OnDrawGizmos);
            grid.m_debugUpdate.AddListener(DebugUpdate);
#endif

            UpdateNode();
        }

        /// <summary>
        /// Calculates the F cost of the node.
        /// </summary>
        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        /// <summary>
        /// Updates the node to check if there are any obstacles in the node.
        /// </summary>
        public void UpdateNode()
        {
            // Get the center of the node as the world position is the bottom left corner
            Vector3 center = grid.GetWorldPosition(x, y);
            center.x += (grid.GetCellSize() / 2);
            center.z += (grid.GetCellSize() / 2);

            // Get the half extents for the Overlap cast, 
            // getting the cell size for x and z and taking half and for the Y we keep the full value
            Vector3 halfExtents = new Vector3(grid.GetCellSize() / 2, grid.GetCellSize(), grid.GetCellSize() / 2);

            // Do a box cast to check if there if there is an obstacle on the grid space
            m_isWalkable = Physics.OverlapBox(center, halfExtents,Quaternion.identity,grid.m_settings.gridObstacles).Length <= 0;            
        }

        /// <summary>
        /// Gets all the colliders inside the node.
        /// </summary>
        /// <returns>An Array with all the colliders in the node.</returns>
        public Collider[] CollidersInNode()
        {
            // Get the center of the node as the world position is the bottom left corner
            Vector3 center = grid.GetWorldPosition(x, y);
            center.x += (grid.GetCellSize() / 2);
            center.z += (grid.GetCellSize() / 2);

            // Get the half extents for the Overlap cast, 
            // getting the cell size for x and z and taking half and for the Y we keep the full value
            Vector3 halfExtents = new Vector3(grid.GetCellSize() / 2, grid.GetCellSize(), grid.GetCellSize() / 2);

            return Physics.OverlapBox(center, halfExtents, Quaternion.identity, grid.m_settings.gridObstacles);
        }

        /// <summary>
        /// Gets the middle of the node in world position.
        /// </summary>
        /// <returns>A Vector3 of world coordinates.</returns>
        public Vector3 GetMiddleWorldPosition()
        {
            // Calculates the center of this node in world position and returns it
            Vector3 center = grid.GetWorldPosition(x, y);
            center.x += (grid.GetCellSize() / 2);
            center.z += (grid.GetCellSize() / 2);
            return center;
        }

        public int CompareTo(PathNode other)
        {
            int compare = fCost.CompareTo(other.fCost);

            if (compare == 0)
            {
                compare = hCost.CompareTo(other.hCost);
            }
            return -compare;
        }

#if (UNITY_EDITOR)
        public void DebugUpdate()
        {

        }

        public void OnDrawGizmos()
        {
            if (grid.m_settings.showGridCollision)
            {
                // Get the center of the node as the world position is the bottom left corner
                Vector3 center = grid.GetWorldPosition(x, y);
                center.x += (grid.GetCellSize() / 2);
                center.z += (grid.GetCellSize() / 2);
                DebugTools.GizmoDrawing.DrawWireCube(center, new Vector3(grid.GetCellSize(), grid.GetCellSize(), grid.GetCellSize()), Color.red);
            }
        }
#endif
    }

}
