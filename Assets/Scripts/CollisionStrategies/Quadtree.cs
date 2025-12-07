using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;


[CreateAssetMenu(menuName = "Collision/Quadtree")]

public class Quadtree : CollisionStrategy
{
    [SerializeField] private int maxObjAmountAllowed = 3;
    [SerializeField] private float maxAreaSize = 1;

    private TreeCell dynamicRootCell;
    private TreeCell staticRootCell;

    private float width;
    private float height;


    Queue<CollisionPair> collisionQueue = new Queue<CollisionPair>();
    Dictionary<CollisionPair, bool> checkedCollisionpairs = new Dictionary<CollisionPair, bool>();

    public override void CheckCollision(List<CollisionObject> objects, List<Line> borders)
    {
        collisionQueue.Clear();
        checkedCollisionpairs.Clear();
        CollisionChecksThisFrame = 0;

        //compute static tree
        if(staticRootCell == null)
        {
            width = borders[0].length;
            height = borders[1].length;
            staticRootCell = new TreeCell(width, height, Vector2.zero);
            //staticRootCell.objects = borders;
        }

        //compute new dynamic tree
        dynamicRootCell = new TreeCell(width, height, borders[0].start);
        dynamicRootCell.objects = objects;
        DrawTreeStructure(dynamicRootCell);

        //chekc collisions
        CheckBallBorderCollisions(objects, borders);
        ComputeCollisionsPerCell(dynamicRootCell);
    }


    //check for possible collision and save in a queue to resolve
    private void ComputeCollisionsPerCell(TreeCell cell)
    {
        collisionQueue.Clear();
        if (cell.isSplit)
        {
            foreach (TreeCell subCell in cell.subCells.Values)
            {
                ComputeCollisionsPerCell(subCell);
            }
        }
        else
        {
            if (cell.objects != null && cell.objects.Count > 1)
            {
                CheckCollisionsInCell(cell.objects);
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
                CollisionChecksThisFrame++;
                if (CircleCircleCollision(A.GetRadius(), A.GetNewPos(), B.GetRadius(), B.GetNewPos()))
                {
                    collisionQueue.Enqueue(new CollisionPair(A, B));
                }
                checkedCollisionpairs.Add(new CollisionPair(A, B), true);
            }
        }
    }

    private void ResolveCollisions()
    {
        //Debug.Log("collisionCount" +  collisionQueue.Count);
        while (collisionQueue.Count > 0)
        {
            CollisionPair collision = collisionQueue.Dequeue();
            CircleCircleResolve(collision);
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


    #region DrawTree
    //check for possible collision and save in a queue to resolve
    private void CheckBallBorderCollisions(List<CollisionObject> objects, List<Line> borders)
    {
        for (int i = 0; i < objects.Count; ++i)
        {
            base.borderCollision(borders, objects[i]);
            //CollisionChecksThisFrame += borders.Count;
        }
    }

    private void DrawTreeStructure(TreeCell originCell)
    {
        if (originCell.isSplitable(maxObjAmountAllowed, maxAreaSize))
        {
            originCell.Split();
            foreach (TreeCell cell in originCell.subCells.Values)
            {
                DrawTreeStructure(cell);
            }
        }
    }


    public override void DebugDraw(List<Line> borders)
    {
        if (dynamicRootCell == null) { return; }
        DrawCell(dynamicRootCell);

    }

    private void DrawCell(TreeCell cell)
    {
        cell.DrawCell(Color.cyan);
        if (cell.isSplit)
        {
            foreach (TreeCell subCell in cell.subCells.Values)
            {
                DrawCell(subCell);
            }
        }
    }

    #endregion
}


public class TreeCell
{
    public List<CollisionObject> objects = new List<CollisionObject>();

    public bool isSplit = false;
    public Dictionary<Vector2Int, TreeCell> subCells = new Dictionary<Vector2Int, TreeCell>(4);

    public Vector2 position;
    public float width;
    public float height;

    public TreeCell(float width, float height, Vector2 worldPosition)
    {
        this.width = width;
        this.height = height;
        this.position = worldPosition;
        isSplit = false;
    }

    public bool isSplitable(int objThreshold, float sizeThreshold)
    {
        if (objects.Count > objThreshold && Mathf.Min(width, height) >= sizeThreshold)
        {
            Debug.Log($"cell is splitable with obj count{objects.Count} and size {Mathf.Min(width, height)}");
            return true;
        }
        Debug.Log($"cell is NOT with obj count{objects.Count} and size {Mathf.Min(width, height)}");
        return false;
    }

    public void AddObject(CollisionObject obj)
    {
        objects.Add(obj);
    }

    public void Split()
    {
        float newWidth = width / 2;
        float newHeight = height / 2;

        subCells.Add(new Vector2Int(0, 0), new TreeCell(newWidth, newHeight, new Vector2(position.x, position.y)));
        subCells.Add(new Vector2Int(1, 0), new TreeCell(newWidth, newHeight, new Vector2(position.x + newWidth, position.y)));
        subCells.Add(new Vector2Int(1, 1), new TreeCell(newWidth, newHeight, new Vector2(position.x + newWidth, position.y + newHeight)));
        subCells.Add(new Vector2Int(0, 1), new TreeCell(newWidth, newHeight, new Vector2(position.x, position.y + newHeight)));

        foreach (CollisionObject obj in objects)
        {
            Vector2 worldPos = obj.GetCurrentPos();
            foreach (Vector2Int XY in GetOverlappedCells(obj, newWidth, newHeight))
            {
                subCells[XY].AddObject(obj);
            }
        }
        objects = null;
        Debug.Log("Cell was spli!");
        isSplit = true;
    }

    public Vector2Int GetSubcellXY(Vector3 worldPos, float newWidth, float newHeight)
    {
        // subtract origin before dividing by cell size
        float localX = worldPos.x - position.x;
        float localY = worldPos.y - position.y;
        Vector2Int coords = new Vector2Int(Mathf.FloorToInt(localX / newWidth), Mathf.FloorToInt(localY / newHeight));
        return new Vector2Int(Mathf.Clamp(coords.x, 0, 1), Mathf.Clamp(coords.y, 0, 1));
    }

    private List<Vector2Int> GetOverlappedCells(CollisionObject obj, float newWidth, float newHeight)
    {
        AABB bb = obj.GetBoundingBoxAtPos(obj.GetCurrentPos());

        Vector2Int min = GetSubcellXY(bb.min, newWidth, newHeight);
        Vector2Int max = GetSubcellXY(bb.max, newWidth, newHeight);

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

    public void DrawCell(Color color)
    {
        Color prevColor = Gizmos.color;
        Gizmos.color = color;

        Vector2 A = new Vector2(position.x, position.y);
        Vector2 B = new Vector2(position.x + width, position.y);
        Vector2 C = new Vector2(position.x + width, position.y + height);
        Vector2 D = new Vector2(position.x, position.y + height);

        Gizmos.DrawLine(A, B);
        Gizmos.DrawLine(B, C);
        Gizmos.DrawLine(C, D);
        Gizmos.DrawLine(D, A);

        Gizmos.color = prevColor;
    }

}