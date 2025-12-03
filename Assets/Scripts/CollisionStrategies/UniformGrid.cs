using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "Collision/Uniform Grid")]

public class UniformGrid : CollisionStrategy
{
    [SerializeField] private float gridSize;
    private float lineGridSize;

    Queue<CollisionPair> collisionQueue = new Queue<CollisionPair>();

    MyGrid<List<CollisionObject>> myGrid;
    MyGrid<List<Line>> myStaticGrid;

    Dictionary<CollisionObject, List<Vector2Int>> GetCurrentCells = new Dictionary<CollisionObject, List<Vector2Int>>();


    Dictionary<KeyValuePair<Line, CollisionObject>, bool> checkedCollisions = new Dictionary<KeyValuePair<Line, CollisionObject>, bool>();
    Dictionary<CollisionPair, bool> checkedCollisionpairs = new Dictionary<CollisionPair, bool>();


    public override void CheckCollision(List<CollisionObject> objects, List<Line> borders)
    {
        collisionQueue.Clear();
        checkedCollisions.Clear();
        checkedCollisionpairs.Clear();

        if (myStaticGrid == null)
        {
            lineGridSize = gridSize * 3;
            myStaticGrid = new MyGrid<List<Line>>(GridType.RECTANGULAR, lineGridSize);
            AddBordersIntoGrid(borders);
        }

        FillDynamicGrid(objects);

        CheckBallBorderCollisions();

        Debug.Log("collisions checked: " + checkedCollisions.Count); 

       // Debug.Log("dictionairy count: " + myGrid.GetGrid().Count);
    }

    private void FillDynamicGrid(List<CollisionObject> objects)
    {
        if (myGrid == null)
        {
            myGrid = new MyGrid<List<CollisionObject>>(GridType.RECTANGULAR, gridSize);
            foreach (CollisionObject obj in objects)
            {
                List<Vector2Int> overlappedcells = GetOverlappedCells(obj.GetCurrentPos(), obj);
                GetCurrentCells.Add(obj, overlappedcells);
                foreach (Vector2Int index in overlappedcells)
                {
                    AddToGrid(index, obj);
                }
            }
        }
        foreach (CollisionObject obj in objects)
        {
            UpdatePositionOnGrid(obj);
            //Debug.Log("new grid size = " + myGrid.GetGrid().Keys.Count);
        }

    }

    private void UpdatePositionOnGrid(CollisionObject obj)
    {
        List<Vector2Int> oldIndexes = new List<Vector2Int>(GetCurrentCells[obj]);
       // Debug.Log("we have this many cells: " + GetCurrentCells[obj].Count);

        List<Vector2Int> newIndexes = GetOverlappedCells(obj.GetNewPos(), obj);

        List<Vector2Int> IndexesToAdd = new List<Vector2Int>();
        List<Vector2Int> IndexesToRemove = new List<Vector2Int>(oldIndexes);

        foreach (Vector2Int index in newIndexes)
        {
            if (!oldIndexes.Contains(index))
            {
                IndexesToAdd.Add(index);
            }
            else
            {
                IndexesToRemove.Remove(index);
            }
        }


        //do we have to add cells
        if (IndexesToAdd.Count > 0)
        {
            foreach (Vector2Int index in IndexesToAdd)
            {
                AddToGrid(index, obj);
            }
        }

        GetCurrentCells[obj].AddRange(IndexesToAdd);


        //do we have to remove cells
        if (IndexesToRemove.Count > 0)
        {
            foreach (Vector2Int index in IndexesToRemove)
            {
                RemoveObjFromOldCell(obj, index);
            }
        }

    }

    private void AddBordersIntoGrid(List<Line> borders)
    {
        foreach (Line line in borders)
        {
            Vector2Int start = myStaticGrid.GetXY(line.start);
            Vector2Int end = myStaticGrid.GetXY(line.end);

            int length = (int)(end - start).magnitude + 1;

            Debug.Log("line length: " + length);

            Vector2Int direction = new Vector2Int(
         (end.x - start.x) != 0 ? (end.x - start.x) / Mathf.Abs(end.x - start.x) : 0,
         (end.y - start.y) != 0 ? (end.y - start.y) / Mathf.Abs(end.y - start.y) : 0
     );
            Debug.Log(direction);

            Vector2Int[] coveredCells = new Vector2Int[length];
            for (int i = 0; i < length; i++)
            {
                coveredCells[i] = new Vector2Int(start.x + (direction.x * i), start.y + (direction.y * i));
                AddToGridLine(coveredCells[i], line);
            }
        }
    }

    private List<Vector2Int> GetOverlappedCells(Vector3 position, CollisionObject obj)
    {
        AABB bb = obj.GetBoundingBoxAtPos(position);

        Vector2Int min = myGrid.GetXY(bb.min);
        Vector2Int max = myGrid.GetXY(bb.max);

        List<Vector2Int> overlapCells = new List<Vector2Int>();

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                overlapCells.Add(new Vector2Int(x, y));
            }
        }

        return overlapCells;
    }

    private void AddToGrid(Vector2Int index, CollisionObject obj)
    {
        if (myGrid.Contains(index))
        {
            if (myGrid.GetValue(index).Contains(obj))
            {
                //Debug.LogError("trying to add object when already added");
                return;
            }
            myGrid.GetValue(index).Add(obj);
        }
        else
        {
            myGrid.SetValue(index, new List<CollisionObject> { obj });
        }
    }

    private void AddToGridLine(Vector2Int index, Line line)
    {
        if (myStaticGrid.Contains(index))
        {
            myStaticGrid.GetValue(index).Add(line);
        }
        else
        {
            myStaticGrid.SetValue(index, new List<Line> { line });
        }
    }

    public override void ClearCollisionStructure()
    {
        GetCurrentCells.Clear();
        myGrid = null;
        myStaticGrid = null;
    }


    //check for possible collision and save in a queue to resolve
    private void CheckBallBorderCollisions()
    {
        collisionQueue.Clear();
        foreach (KeyValuePair<Vector2Int, List<CollisionObject>> keyValuePair in myGrid.GetGrid())
        {
            Vector3 cellWorldPosition = myGrid.GetWorldPosition(keyValuePair.Key);
            bool hasStaticObjects = myStaticGrid.Contains(myStaticGrid.GetXY(cellWorldPosition));
            if (hasStaticObjects)
            {
                foreach (CollisionObject obj in keyValuePair.Value)
                {
                    foreach (Line line in myStaticGrid.GetValue(myStaticGrid.GetXY(cellWorldPosition)))
                    {
                        KeyValuePair<Line, CollisionObject> collisionInstance = new KeyValuePair<Line, CollisionObject>(line, obj);
                        if (!checkedCollisions.ContainsKey(collisionInstance))
                        {
                            checkedCollisions.Add(new KeyValuePair<Line, CollisionObject>(line, obj), true);
                            CircleLineCollision(line, obj);
                        }
                    }
                }
            }
            if (keyValuePair.Value.Count > 1)
            {
                CheckCollisionsInCell(keyValuePair.Value);
            }
        }
        if (collisionQueue.Count > 0)
        {
            ResolveCollisions();
        }
    }

    private void CheckCollisionsInCell(List<CollisionObject> objects)
    {
        for (int i = 0; i < objects.Count; ++i)
        {
            for (int j = 0; j < objects.Count; ++j)
            {
                if (i == j) { continue; }
                CollisionObject A = objects[i];
                CollisionObject B = objects[j];
                CollisionPair pair = new CollisionPair(A, B);
                if (HasPairBeenChecked(pair)) { Debug.LogWarning("rejected"); continue; }
                if (CircleCircleCollision(A.GetRadius(), A.GetNewPos(), B.GetRadius(), B.GetNewPos()))
                {
                    collisionQueue.Enqueue(new CollisionPair(A, B));
                }
                checkedCollisionpairs.Add(new CollisionPair(A, B), true);
            }
        }
    }

    private bool HasPairBeenChecked(CollisionPair pair)
    {
        if (checkedCollisionpairs.ContainsKey(pair))
        {
            Debug.LogWarning("actually checking it only once :)");
            return true;
        }
        return false;
    }

    private void RemoveObjFromOldCell(CollisionObject obj, Vector2Int myIndex)
    {
        if (myGrid.Contains(myIndex))
        {
            List<CollisionObject> cellContent = myGrid.GetValue(myIndex);
            //if(cellContent.Contains(obj)) { Debug.LogWarning("does not contain the object"); }
            //if(!cellContent.Remove(obj)) { Debug.LogWarning("could not be removed"); }
            cellContent.Remove(obj);

            //remove cell if empty
            if (cellContent.Count == 0)
            {
                //Debug.Log("Deleted key");
                myGrid.RemoveIndex(myIndex);
            }
        }
        GetCurrentCells[obj].Remove(myIndex);
    }

    //resolve collisions
    private void ResolveCollisions()
    {
        Debug.Log("collisionCount" +  collisionQueue.Count);
        while (collisionQueue.Count > 0)
        {
            CollisionPair collision = collisionQueue.Dequeue();
            CircleCircleResolve(collision);
        }
    }

    public override void DebugDraw(List<Line> borders)
    {



        Gizmos.color = Color.blue;
        if (myStaticGrid != null)
        {
            Dictionary<Vector2Int, List<Line>> grid = myStaticGrid.GetGrid();
            float _cellSize = myStaticGrid.GetCellSize();

            foreach (Vector2Int index in grid.Keys)
            {
                Vector3 pos = myStaticGrid.GetWorldPosition(index);
                Gizmos.DrawSphere(pos, 0.1f);
                Gizmos.DrawLine(pos, new Vector3(pos.x, pos.y + _cellSize, pos.z));
                Gizmos.DrawLine(new Vector3(pos.x, pos.y + _cellSize, pos.z), new Vector3(pos.x + _cellSize, pos.y + _cellSize, pos.z));
                Gizmos.DrawLine(new Vector3(pos.x + _cellSize, pos.y + _cellSize, pos.z), new Vector3(pos.x + _cellSize, pos.y, pos.z));
                Gizmos.DrawLine(new Vector3(pos.x + _cellSize, pos.y, pos.z), pos);
                Handles.Label(new Vector3(pos.x + _cellSize / 2, pos.y + _cellSize / 2, 0), grid[index].Count.ToString());
            }
        }

        Gizmos.color = Color.yellow;
        if (myGrid != null)
        {
            Dictionary<Vector2Int, List<CollisionObject>> grid = myGrid.GetGrid();
            float _cellSize = myGrid.GetCellSize();

            foreach (Vector2Int index in grid.Keys)
            {
                Vector3 pos = myGrid.GetWorldPosition(index);
                Gizmos.DrawSphere(pos, 0.1f);
                Gizmos.DrawLine(pos, new Vector3(pos.x, pos.y + _cellSize, pos.z));
                Gizmos.DrawLine(new Vector3(pos.x, pos.y + _cellSize, pos.z), new Vector3(pos.x + _cellSize, pos.y + _cellSize, pos.z));
                Gizmos.DrawLine(new Vector3(pos.x + _cellSize, pos.y + _cellSize, pos.z), new Vector3(pos.x + _cellSize, pos.y, pos.z));
                Gizmos.DrawLine(new Vector3(pos.x + _cellSize, pos.y, pos.z), pos);
                Handles.Label(new Vector3(pos.x + _cellSize / 2, pos.y + _cellSize / 2, 0), grid[index].Count.ToString());
            }
        }

        Gizmos.color = Color.white;
    }


}
