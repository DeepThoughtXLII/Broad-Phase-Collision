using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class CollisionStrategy : ScriptableObject
{
    
    //override this for different broad phase collision
    public virtual void CheckCollision(List<CollisionObject> objects, List<Line> borders)
    {

    }


    #region circle circle collision
    //returns true if two balls collide + false if they do not
    public bool CircleCircleCollision(float r1, Vector2 pos1, float r2, Vector2 pos2)
    {
        float distance = Vector2.Distance(pos1, pos2);
        if (distance - (r1 + r2) > 0)
        {
            return false;
        }
        return true;
    }

    //returns the reset position of a ball that collided
    private Vector2 GetNewCirclePosition(float r1, Vector2 pos1, float r2, Vector2 pos2)
    {
        Vector2 directionVec = pos2 - pos1;
        float optimalLength = r1 + r2;
        float diffLength = optimalLength - directionVec.magnitude;
        return pos1 - diffLength * directionVec.normalized;
    }


    //Resets the position of a circle circle collision pair and reflects their directions
    public void CircleCircleResolve(CollisionPair collision)
    {
        Vector2 newPosition = GetNewCirclePosition(collision.objectA.GetRadius(), collision.objectA.GetPosition(), collision.objectB.GetRadius(), collision.objectB.GetPosition());
        collision.objectA.SetNewPosition(newPosition);

        Vector2 lineVector = collision.objectA.GetPosition() - collision.objectB.GetPosition();
        collision.objectA.ReflectDirection(-lineVector.normalized);
    }

    #endregion

    #region circle line collision

    //collision between circle and line
    public void borderCollision(List<Line> borders, CollisionObject obj)
    {
        foreach (Line border in borders)
        {
            if (ballDistance(obj.GetPosition(), border) < obj.GetRadius())
            {
                obj.SetNewPosition(POI(obj.GetPosition(), obj.GetVelocity(), obj.GetRadius(), border));

                obj.ReflectDirection(Vector2.Perpendicular(border.lineVector).normalized);
            }
        }
    }

    //returns distance of ball to a line with scalar projection
    private float ballDistance(Vector2 ballPos, Line line)
    {
        Vector2 differenceVec = ballPos - line.start;
        float distanceToLine = Vector2.Dot(differenceVec, Vector2.Perpendicular(line.lineVector));
        return distanceToLine;
    }

    //returns the time of impact of a ball on a line
    //time of impact = at what point in a frame exactly would the ball have collided with the line?
    private float TOI(Vector2 oldBallPos, Vector2 ballVelocity, float ballRadius, Line line)
    {
        float a = ballDistance(oldBallPos, line) - ballRadius;                             //the distance between the oldPosition of the ball and the position the ball would have, if it were to collide with the line
        float b = Vector2.Dot(-ballVelocity, Vector2.Perpendicular(line.lineVector));       //the distance between the oldPosition of the ball and the newPosition (behind the line). 
        float t = a / b;                                                                     //time of impact calculation
        return t;
    }

    //returns the point of impact of a ball onto a line
    //basically going from the old position of the ball we add the fraction of velocity/distance the ball would have made until hitting the line (thanks to TOI) 
    private Vector2 POI(Vector2 oldBallPos, Vector2 ballVelocity, float ballradius, Line line)
    {
        Vector2 poi = oldBallPos + TOI(oldBallPos, ballVelocity, ballradius, line) * ballVelocity;  //mathMagic
        return poi;
    }

    #endregion
}



//helper for the broad phase collision
public class CollisionPair
{
    public CollisionObject objectA;
    public CollisionObject objectB;


    public CollisionPair(CollisionObject objectA, CollisionObject objectB)
    {
        this.objectA = objectA;
        this.objectB = objectB;
    }
}