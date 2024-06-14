


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Collision")]

public class LinearCollision : CollisionStrategy
{
    Queue<CollisionPair> collisionQueue = new Queue<CollisionPair>();
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


    //check for possible collision and save in a queue to resolve
    private void CheckBallBorderCollisions(List<CollisionObject> objects)
    {
        int collisionChecks = objects.Count * objects.Count;
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
                    collisionQueue.Enqueue(new CollisionPair(A, B));
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


    //resolve collisions
    private void ResolveCollisions()
    {
        while (collisionQueue.Count > 0)
        {
            CollisionPair collision = collisionQueue.Dequeue();
            CircleCircleResolve(collision);
        }

        if (tempObjects.Count > 0)
        {
            Debug.Log(tempObjects.Count + " objects to check");
            CheckBallBorderCollisions(tempObjects);
        }
    }






}


