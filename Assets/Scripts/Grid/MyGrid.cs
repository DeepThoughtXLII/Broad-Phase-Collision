using System.Collections.Generic;
using UnityEditor;


namespace UnityEngine
{

    public class MyGrid<TGridObject>
    {
        #region fields

        private Vector3 origin;

        private float _cellSize;

        private Dictionary<Vector2Int, TGridObject> grid;

        private GridType type = GridType.RECTANGULAR;

        Vector2 iHat;
        Vector2 jHat;
        #endregion


        //constructor, assigning the parameters to values
        public MyGrid(GridType type, float cellSize)
        {
            this.type = type;
            _cellSize = cellSize;

            iHat = new Vector2(cellSize, cellSize / 2); //   1, 0.5
            jHat = new Vector2(-cellSize, cellSize / 2);//  -1, 0.5

            grid = new Dictionary<Vector2Int, TGridObject>();
        }



        #region converting positions
        //returns the Worldposition by getting the gridTile
        public Vector3 GetWorldPosition(Vector2Int position)
        {
            switch (type)
            {
                case GridType.RECTANGULAR:
                    return new Vector3(position.x, position.y) * _cellSize;
                case GridType.ISOMETRIC:
                    Vector2 pos = new Vector2(position.x, position.y) * _cellSize;
                    float xPos = pos.x * iHat.x + pos.y * jHat.x;
                    float yPos = pos.x * iHat.y + pos.y * jHat.y;
                    return origin + new Vector3(xPos, yPos);
            }
            throw new System.Exception("No Grid Type selected");
        }

        //returns the grid position based on the given world position
        public Vector2Int GetXY(Vector3 worldPosition)
        {
            switch (type)
            {
                case GridType.RECTANGULAR:
                    return new Vector2Int(Mathf.FloorToInt(worldPosition.x / _cellSize), Mathf.FloorToInt(worldPosition.y / _cellSize));
                case GridType.ISOMETRIC:
                    Vector3 screen = worldPosition - origin;
                    Vector2Int map = Vector2Int.zero;
                    float halfTileWidth = _cellSize;
                    float halfTileHeight = _cellSize / 2;

                    map.x = (int)(screen.x / halfTileWidth + screen.y / halfTileHeight) / 2;
                    map.y = (int)(screen.y / halfTileHeight - (screen.x / halfTileWidth)) / 2;
                    //Debug.DrawLine(worldPosition, GetWorldPosition(map), Color.red, 2f);
                    return new Vector2Int(map.x, map.y);
            }
            throw new System.Exception("No Grid Type selected");
        }

        #endregion

        #region getters and setters
        //returns the grid array
        public Dictionary<Vector2Int, TGridObject> GetGrid()
        {
            return grid;
        }

        public void RemoveIndex(Vector2Int index)
        {
            grid.Remove(index);
        }

        ////returns width and height of grid in cells
        //public Vector2Int GetDimensions()
        //{
        //    return new Vector2Int(_width, _height);
        //}


        //returns cellSize in units
        public float GetCellSize()
        {
            return _cellSize;
        }


        //sets the cellsize in units
        public void SetCellSize(float cellsize)
        {
            _cellSize = cellsize;
        }

        //returns gridobject at position
        public bool Contains(Vector2Int index)
        {
            if (grid.ContainsKey(index))
            {
                return true;
            }
            return false;
        }

        public TGridObject GetValue(Vector2Int index)
        {
            return grid[index];
        }

        #endregion

        #region saving gridobjects
        //saves the gridobject at the given position of the grid
        public void SetValue(Vector2Int index, TGridObject value)
        {
            if (!grid.ContainsKey(index))
            {
                grid.Add(index, value);
            }
        }


        //saves the gridobject but by worldposition instead
        public void SetValue(Vector3 worldPosition, TGridObject value)
        {
            Vector2Int pos = GetXY(worldPosition);
            SetValue(pos, value);
        }
        #endregion

        #region changing the grid
        //sets the origin of the grid to a new position
        public void SetGridOrigin(Vector3 newOrigin)
        {
            origin = newOrigin;
        }


        ////changes the size of the array to match the new grid
        //public Vector2Int[] ChangeArraySize(Vector2Int size)
        //{
        //    TGridObject[,] newGrid = new TGridObject[size.x, size.y];
        //    int deprecatedElements = (_width * _height) - (size.x * size.y);
        //    Vector2Int[] deprecatedIndexes = null;
        //    bool hasDeprecatedElm = false;
        //    int depElmIterator = 0;
        //    if (deprecatedElements > 0)
        //    {
        //        hasDeprecatedElm = true;
        //        deprecatedIndexes = new Vector2Int[deprecatedElements];
        //    }
        //    int maxWidth = newGrid.GetLength(0);
        //    int maxHeight = newGrid.GetLength(1);
        //    int minWidth = grid.GetLength(0);
        //    int minHeight = grid.GetLength(1);
        //    if (size.x < _width)
        //    {
        //        maxWidth = grid.GetLength(0);
        //        minWidth = newGrid.GetLength(0);
        //    }
        //    if (size.y < _height)
        //    {
        //        maxHeight = grid.GetLength(1);
        //        minHeight = newGrid.GetLength(1);
        //    }
        //    for (int x = 0; x < maxWidth; x++)
        //    {
        //        for (int y = 0; y < maxHeight; y++)
        //        {
        //            if (hasDeprecatedElm && x > minWidth - 1 || hasDeprecatedElm && y > minHeight - 1)
        //            {
        //                deprecatedIndexes[depElmIterator] = new Vector2Int(x, y);
        //                depElmIterator++;
        //            }
        //            else if (x < minWidth && y < minHeight)
        //            {
        //                newGrid[x, y] = grid[x, y];
        //            }
        //        }
        //    }
        //    grid = newGrid;
        //    _width = size.x;
        //    _height = size.y;
        //    return deprecatedIndexes;
        //}

        #endregion


        public void DrawGrid()
        {
            foreach (Vector2Int index in grid.Keys)
            {
                Vector3 pos = GetWorldPosition(index);
                Gizmos.DrawSphere(pos, 0.1f);
                Gizmos.DrawLine(pos, new Vector3(pos.x, pos.y + _cellSize, pos.z));
                Gizmos.DrawLine(new Vector3(pos.x, pos.y + _cellSize, pos.z), new Vector3(pos.x + _cellSize, pos.y + _cellSize, pos.z));
                Gizmos.DrawLine(new Vector3(pos.x + _cellSize, pos.y + _cellSize, pos.z), new Vector3(pos.x + _cellSize, pos.y, pos.z));
                Gizmos.DrawLine(new Vector3(pos.x + _cellSize, pos.y, pos.z), pos);
#if UNITY_EDITOR

                Handles.Label(new Vector3(pos.x, pos.y + _cellSize / 4 * 3, 0), grid[index].ToString());
                #endif
            }
            //if (grid.Length > 0)
            //{
            //    for (int x = 0; x < grid.GetLength(0); x++)
            //    {
            //        for (int y = 0; y < grid.GetLength(1); y++)
            //        {
            //            Gizmos.DrawSphere(GetWorldPosition(new Vector2Int(x, y)), 0.1f);
            //            Vector3 WorldPos = GetWorldPosition(new Vector2Int(x, y));
            //            Gizmos.DrawLine(WorldPos, GetWorldPosition(new Vector2Int(x, y + 1)));
            //            Gizmos.DrawLine(WorldPos, GetWorldPosition(new Vector2Int(x + 1, y)));
            //        }
            //    }
            //    Gizmos.DrawLine(GetWorldPosition(new Vector2Int(0, _height)), GetWorldPosition(new Vector2Int(_width, _height)));
            //    Gizmos.DrawLine(GetWorldPosition(new Vector2Int(_width, 0)), GetWorldPosition(new Vector2Int(_width, _height)));
            //}
        }

    }

}

public enum GridType { RECTANGULAR, ISOMETRIC };