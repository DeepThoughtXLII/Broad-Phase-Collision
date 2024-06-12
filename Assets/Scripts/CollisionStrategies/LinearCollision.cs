


using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Collision")]

public class LinearCollision : CollisionStrategy
{
    Queue<Collision> collisionQueue = new Queue<Collision>();
    List<Line> borders;

    List<CollisionObject> tempObjects = new List<CollisionObject>();

    public override void CheckCollision(List<CollisionObject> objects, List<Line> borders)
    {
        Debug.Log("new collision check started.");
        collisionQueue.Clear();
        tempObjects.Clear();
        this.borders = borders;
        CheckBallBorderCollisions(objects);
    }

    public void CheckBallBorderCollisions(List<CollisionObject> objects)
    {
        int collisionChecks = objects.Count * objects.Count;
        //Debug.Log(collisionChecks + " collision checks to go");
        collisionQueue.Clear();

        for (int i = 0; i < objects.Count; ++i)
        {
            base.borderCollision(borders, objects[i]);

            for (int j = 0; j < objects.Count; ++j)
            {
                if (i == j) { continue; }
                CollisionObject A = objects[i];
                CollisionObject B = objects[j];

                if (base.CircleCircleCollision(A.GetRadius(), A.GetPosition(), B.GetRadius(), B.GetPosition()))
                {
                    collisionQueue.Enqueue(new Collision(A, B));
                }
            }
        }
        if (collisionQueue.Count > 0)
        {
            tempObjects.Clear();
            Debug.Log("queue has " + collisionQueue.Count + "checks to go");
            ResolveCollisions();
        }
    }

    public void ResolveCollisions()
    {
        while (collisionQueue.Count > 0)
        {
            Collision collision = collisionQueue.Dequeue();
            CircleCircleResolve(collision);
        }

        if (tempObjects.Count > 0)
        {
            Debug.Log(tempObjects.Count + " objects to check");
            CheckBallBorderCollisions(tempObjects);
        }
    }

    public void CircleCircleResolve(Collision collision)
    {
        Vector2 newPosition = base.GetNewCirclePosition(collision.objectA.GetRadius(), collision.objectA.GetPosition(), collision.objectB.GetRadius(), collision.objectB.GetPosition());
        collision.objectA.SetNewPosition(newPosition);

        Vector2 lineVector = collision.objectA.GetPosition() - collision.objectB.GetPosition();

        //Vector2 normalOfLineVector = Vector2.Perpendicular(lineVector);
        //Vector2 normalOfLinevectorNormal = Vector2.Perpendicular(normalOfLineVector);

        //Vector2 start = collision.objectA.GetPosition() + lineVector.normalized * lineVector.magnitude / 2;
        //Debug.DrawLine(collision.objectA.GetPosition(), collision.objectB.GetPosition());
        //Debug.DrawLine(start, start + normalOfLineVector.normalized);

        collision.objectA.ReflectDirection(-lineVector.normalized);

        tempObjects.Add(collision.objectA);
    }




}


public class Collision
{
    public CollisionObject objectA;
    public CollisionObject objectB;


    public Collision(CollisionObject objectA, CollisionObject objectB)
    {
        this.objectA = objectA;
        this.objectB = objectB;
    }
}