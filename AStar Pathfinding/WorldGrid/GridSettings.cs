using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGrid
{
    [CreateAssetMenu(fileName = "Grid settings", menuName = "Grid/Grid settings")]
    public class GridSettings : ScriptableObject
    {
        [Header("Grid settings")]
        public int gridWidth = 1; //The amount of cells there are in this grid in width
        public int gridHeight = 1; //The amount of cells there are in this grid in height
        public float cellSize = 1;//How big each cell is
        public Vector3 originPosition = new Vector3(0,0,0); //Where the grid starts getting generated
        public LayerMask gridObstacles; //Layer(s) where obstacles are located that could block cells
        [Header("Debug settings")]
        public bool showGrid = true;
        public bool showGridPosition = false;
        public bool showGridCollision = false;        
    }
}
