using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WorldGrid
{
    public class Grid<TGridObject>
    {
        public GridSettings m_settings;
        public TGridObject[,] m_gridArray;
        public UnityEvent m_onGizmoDraw;
        public UnityEvent m_debugUpdate;
        public Grid(int _width, int _height, float _cellSize, Vector3 _originPosition, LayerMask _obstacleLayers ,Func<int, int, Grid<TGridObject>, TGridObject> _createGridObject)
        {
            m_settings = new GridSettings();
            m_settings.gridWidth = _width;
            m_settings.gridHeight = _height;
            m_settings.cellSize = _cellSize;
            m_settings.originPosition = _originPosition;
            m_settings.gridObstacles = _obstacleLayers;

            m_gridArray = new TGridObject[m_settings.gridWidth, m_settings.gridHeight];
            m_onGizmoDraw = new UnityEvent();
            m_debugUpdate = new UnityEvent();

            for (int x = 0; x < m_gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < m_gridArray.GetLength(1); y++)
                {
                    m_gridArray[x, y] = _createGridObject(x, y,this);
                }
            }
        }
        public Grid(GridSettings settings, Func<int, int, Grid<TGridObject>, TGridObject> _createGridObject)
        {
            m_settings = settings;

            m_gridArray = new TGridObject[m_settings.gridWidth, m_settings.gridHeight];

            m_onGizmoDraw = new UnityEvent();
            m_debugUpdate = new UnityEvent();
            for (int x = 0; x < m_gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < m_gridArray.GetLength(1); y++)
                {
                    m_gridArray[x, y] = _createGridObject(x, y, this);
                }
            }
        }
        public int GetWidth()
        {
            return m_settings.gridWidth;
        }
        public int GetHeight()
        {
            return m_settings.gridHeight;
        }
        public float GetCellSize()
        {
            return m_settings.cellSize;
        }
        public Vector3 GetWorldPosition(int x, int y)
        {
            //return cell world position
            return new Vector3(x, 0, y) * m_settings.cellSize + m_settings.originPosition;
        }
        private Vector2Int GetXY(Vector3 worldPosition)
        {
            //Get the grid position based on world position
            return new Vector2Int(
            Mathf.FloorToInt((worldPosition - m_settings.originPosition).x / m_settings.cellSize),
            Mathf.FloorToInt((worldPosition - m_settings.originPosition).z / m_settings.cellSize)
            );
        }
        public TGridObject GetGridObject(Vector2Int gridPos)
        {
            //Get object in grid based on grid position
            if (gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < m_settings.gridWidth && gridPos.y < m_settings.gridHeight)
            {
                return m_gridArray[gridPos.x, gridPos.y];
            }
            else
            {
                return default(TGridObject);
            }
        }
        public TGridObject GetGridObject(Vector3 worldPosition)
        {
            //Get grid object based on world position by converting it first
            Vector2Int gridPos = GetXY(worldPosition);
            return GetGridObject(gridPos);
        }
        public TGridObject GetGridObject(int x, int y)
        {
            //Get grid object based on loose X Y cords in the grid            
            return GetGridObject(new Vector2Int(x,y));
        }

#if (UNITY_EDITOR)

        public void DebugUpdate()
        {
            m_debugUpdate.Invoke();
        }
        public void OnDrawGizmos()
        {
            if (m_settings.showGrid)
            {
                for (int x = 0; x < m_settings.gridWidth; x++)
                {
                    for (int y = 0; y < m_settings.gridHeight; y++)
                    {
                        Vector3 worldPos = GetWorldPosition(x, y);
                        Vector3[] gridCorners =
                        {
                            //Bottom left corner
                            worldPos,
                            //Bottom right corner
                            new Vector3(worldPos.x +  m_settings.cellSize, worldPos.y,worldPos.z),                       
                            //Top right corner
                            new Vector3(worldPos.x +  m_settings.cellSize, worldPos.y ,worldPos.z +  m_settings.cellSize),
                            //Top left corner
                            new Vector3(worldPos.x, worldPos.y,worldPos.z +  m_settings.cellSize)
                        };
                        DebugTools.GizmoDrawing.DrawSquareOutline(gridCorners, Color.black);
                        if (m_settings.showGridPosition)
                        {
                            //Get a position just a little off the left side on grid cell
                            Vector3 labelPos = new Vector3(worldPos.x + 0.1f, worldPos.y, worldPos.z + m_settings.cellSize / 2);
                            DebugTools.GizmoDrawing.DrawLabel(labelPos, x + "X " + y + "Y",Color.black);
                        }
                    }
                }
            }

            m_onGizmoDraw.Invoke();
        }
#endif
    }
}
