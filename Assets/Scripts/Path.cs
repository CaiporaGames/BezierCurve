using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
   [SerializeField,HideInInspector]
    List<Vector2> points;   

    public Path(Vector2 center)
    {
        points = new List<Vector2>
        {
            center+Vector2.left,
            center+(Vector2.left+Vector2.up)*0.5f,
            center+(Vector2.right+Vector2.down)*0.5f,
            center+Vector2.right
        };
    }

    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public int NumPoints
    {
        get
        {
            return points.Count;
        }
    }

    public int NumSegments
    {
        get 
        {
            return (points.Count-4)/3+1;
            //return (int)((points.Count-4)*0.334f+1);//Slightly optimization
        }
    }

    public void AddSegments(Vector2 anchorPos)
    {
        points.Add(points[points.Count-1]*2-points[points.Count-2]);
        points.Add(points[points.Count-1]+anchorPos*0.5f);
        points.Add(anchorPos);
    }

    public Vector2[] GetPointsInSegment(int i) 
    {
        return new Vector2[]
        {points[i*3], points[i*3+1], points[i*3+2], points[i*3+3]};
    }

    public void MovePoint(int i, Vector2 newPos)
    {
        Vector2 deltaMove = newPos - points[i];//How much our point moved
        points[i] = newPos;

        if(i%3==0)//Our point is an anchorPoint
        {
            if(i+1<points.Count)//last point does not exists
            {
                points[i+1] += deltaMove;
            }
            if(i-1>=0)//There is no point before the first one.
            {
                points[i-1] += deltaMove;
            }
        }
        else
        {
            bool nextPointIsAnchor = (i+1)%3 == 0;

            int correspondingControlIndex = nextPointIsAnchor ? i+2 : i-2;
            int anchorIndex = nextPointIsAnchor ? i+1 : i-1;

            if(correspondingControlIndex >= 0 && correspondingControlIndex < points.Count)
            {
                //Help to keep the distance between the acnhor point and the controls points
                float distance = (points[anchorIndex]-points[correspondingControlIndex]).magnitude;
                Vector2 direction = (points[anchorIndex]-newPos).normalized;
                points[correspondingControlIndex] = points[anchorIndex]+direction * distance;
            }
        }
    }
}