using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class CollisionStrategy : ScriptableObject
{
    

    public virtual void CheckCollision(List<CollisionObject> objects, List<Line> borders)
    {

    }

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

    public float ballDistance(Vector2 ballPos, Line line)
    {
        Vector2 differenceVec = ballPos - line.start;                                        
        float distanceToLine = Vector2.Dot(differenceVec, Vector2.Perpendicular(line.lineVector));
        return distanceToLine;                                                          
    }

    //------------------------------------------------------------------------------------------------------------------------
    //														TOI()
    //------------------------------------------------------------------------------------------------------------------------
    //returns the time of impact of a ball on a line
    //time of impact = at what point in a frame exactly would the ball have collided with the line?
    public float TOI(Vector2 oldBallPos, Vector2 ballVelocity, float ballRadius, Line line)
    {
        float a = ballDistance(oldBallPos, line) - ballRadius;                             //the distance between the oldPosition of the ball and the position the ball would have, if it were to collide with the line
        float b = Vector2.Dot(-ballVelocity, Vector2.Perpendicular(line.lineVector));                     //the distance between the oldPosition of the ball and the newPosition (behind the line). 
        float t = a / b;                                                                     //time of impact calculation
        return t;
    }






    //------------------------------------------------------------------------------------------------------------------------
    //														POI()
    //------------------------------------------------------------------------------------------------------------------------
    //returns the point of impact of a ball onto a line
    //basically going from the old position of the ball we add the fraction of velocity/distance the ball would have made until hitting the line (thanks to TOI) 
    public Vector2 POI(Vector2 oldBallPos, Vector2 ballVelocity, float ballradius, Line line)
    {
        Vector2 poi = oldBallPos + TOI(oldBallPos, ballVelocity, ballradius, line) * ballVelocity;  //mathMagic
        return poi;
    }

    public bool CircleCircleCollision(float r1, Vector2 pos1, float r2, Vector2 pos2)
    {
        float distance = Vector2.Distance(pos1, pos2);
        if(distance - (r1 + r2) > 0)
        {
            return false;
        }
        return true;
    }

    public Vector2 GetNewCirclePosition(float r1, Vector2 pos1, float r2, Vector2 pos2)
    {
        Vector2 directionVec = pos2 - pos1;
        float optimalLength = r1 + r2;
        float diffLength = optimalLength - directionVec.magnitude;
        return pos1 - diffLength * directionVec.normalized;
    }
}
