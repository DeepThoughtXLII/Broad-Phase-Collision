using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    [SerializeField] private int objectAmount = 10;
    [SerializeField] private GameObject objectPrefab;

    private Vector2 xBounds;
    private Vector2 yBounds;
    [SerializeField] private float Frame = 1;

    List<CollisionObject> myObjects = new List<CollisionObject>();
    List<Line> borders = new List<Line>();

    [SerializeField] private CollisionStrategy myCollisionStrategy;


    [SerializeField] private CollisionObject manualObject;


    private void Start()
    {
        GetBounds();
        SpawnObjectsInBounds();
        if(manualObject != null) { myObjects.Add(manualObject); }
    }

    private void FixedUpdate()
    {
        
        foreach (CollisionObject obj in myObjects)
        {
            obj.Step(Time.fixedDeltaTime);
        }
        myCollisionStrategy.CheckCollision(myObjects, borders);

        
    }

    private void GetBounds()
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize * 2;
        float camWidth = cam.aspect * camHeight;

        xBounds.x = cam.transform.position.x - camWidth / 2 + Frame;
        xBounds.y = cam.transform.position.x + camWidth / 2 - Frame;
        yBounds.x = cam.transform.position.y - camHeight / 2 + Frame;
        yBounds.y = cam.transform.position.y + camHeight / 2 - Frame;

        Vector2 point1 = new Vector2(xBounds.x, yBounds.x);
        Vector2 point2 = new Vector2(xBounds.y, yBounds.x);
        Vector2 point3 = new Vector2(xBounds.y, yBounds.y);
        Vector2 point4 = new Vector2(xBounds.x, yBounds.y);


        borders.Add(new Line(point1, point2));
        borders.Add(new Line(point2, point3));
        borders.Add(new Line(point3, point4));
        borders.Add(new Line(point4, point1));
    }
    private void SpawnObjectsInBounds()
    {
        for(int i = 0; i < objectAmount; i++)
        {
            Vector3 newPos = GetPosOnEvenGrid();
            CollisionObject newObject = Instantiate(objectPrefab, newPos, Quaternion.identity, transform).GetComponent<CollisionObject>();
            myObjects.Add(newObject);
        }
    }

    private Vector3 GetPosOnExactGrid()
    {
        Vector3 pos = new Vector3();

        float unitPerObject = objectPrefab.transform.localScale.x + 0.2f;

        int column = (int)((borders[0].end - borders[0].start).magnitude / unitPerObject);
        int row = (int)((borders[1].end - borders[1].start).magnitude / unitPerObject);

        Debug.Log("row: " + row + " and column: " + column);
        int index = myObjects.Count;

        Vector2 upperLCorner = new Vector2(xBounds.x, yBounds.y);

        int indexX = index % column;
        int indexY = index / column;

        pos.x = upperLCorner.x + unitPerObject + unitPerObject * indexX;
        pos.y = (upperLCorner.y - unitPerObject) - unitPerObject * indexY;


        return pos;
    }

    Vector2Int CalculateOptimalGrid(int totalObjects, float boxWidth, float boxHeight)
    {
        // Calculate the approximate number of columns based on the box aspect ratio
        float boxAspectRatio = boxWidth / boxHeight;
        int columns = Mathf.RoundToInt(Mathf.Sqrt(totalObjects * boxAspectRatio));
        int rows = Mathf.CeilToInt((float)totalObjects / columns);

        // Adjust columns and rows to better fit the aspect ratio
        while (columns * rows < totalObjects)
        {
            if ((float)columns / rows > boxAspectRatio)
            {
                rows++;
            }
            else
            {
                columns++;
            }
        }

        return new Vector2Int(columns, rows);
    }

    private Vector3 GetPosOnEvenGrid()
    {
        Vector3 pos = new Vector3();

        float unitPerObject = objectPrefab.transform.localScale.x + 0.2f;

        float availableWidth = (borders[0].end - borders[0].start).magnitude;
        float availableHeight = (borders[1].end - borders[1].start).magnitude;

        Vector2Int grid = CalculateOptimalGrid(objectAmount, availableWidth, availableHeight);

        float rowSpace = availableHeight / grid.y;
        float columnSpace = availableWidth / grid.x;    

        Debug.Log("row: " + grid.x + " and column: " + grid.y);
        int index = myObjects.Count;

        Vector2 upperLCorner = new Vector2(xBounds.x, yBounds.y);

        int indexX = index % grid.x;
        int indexY = index / grid.x;

        pos.x = (upperLCorner.x + unitPerObject) + columnSpace * indexX;
        pos.y = (upperLCorner.y - unitPerObject) - rowSpace * indexY;


        return pos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if(borders.Count <= 0) { GetBounds(); }

        for(int i = 0; i < borders.Count; i++)
        {
            Gizmos.DrawLine(borders[i].start, borders[i].end);
        }

        Gizmos.color = Color.red;

        foreach(Line border in borders)
        {
            var length = (border.end - border.start).magnitude;
            var start = border.start + length / 2 * border.lineVector;
            Gizmos.DrawLine(start, start + Vector2.Perpendicular(border.lineVector).normalized);
        }

        Gizmos.color = Color.white;
    }


}


public struct Line
{
    public Vector2 start;
    public Vector2 end;
    public Vector2 lineVector;

    public Line(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
        lineVector = (end - start).normalized;
    }
}