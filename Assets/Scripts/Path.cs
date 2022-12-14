using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
   [SerializeField,HideInInspector]
    List<Vector2> points;   
   [SerializeField,HideInInspector]
    bool isClosed = false;
    [SerializeField, HideInInspector]
    bool autoSetControlsPoints = false;

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

    public bool IsClosed
    {
        get
        {
            return isClosed;
        }
        set
        {
            if(isClosed != value)
            {
                isClosed = value;

                if(isClosed)
                {
                    points.Add(points[points.Count-1]*2-points[points.Count-2]);
                    points.Add(points[0]*2-points[1]);

                    if(autoSetControlsPoints)
                    {
                        AutoSetAnchorControlPoint(0);
                        AutoSetAnchorControlPoint(points.Count-3);
                    }
                }
                else
                {
                    points.RemoveRange(points.Count-2,2);
                    if(autoSetControlsPoints)
                        AutoSetStartAndEndControls();
                }
            }
        }
    }

    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public bool AutoSetControlsPoints
    {
        get
        {
            return autoSetControlsPoints;
        }
        set
        {
            if(autoSetControlsPoints != value)
            {
                autoSetControlsPoints = value;
                if(autoSetControlsPoints)
                {
                    AutoSetAllControlsPoints();
                }
            }
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
            return points.Count/3;
            //return (int)points.Count*0.334f;//Slightly optimization
        }
    }

    public void AddSegments(Vector2 anchorPos)
    {
        points.Add(points[points.Count-1]*2-points[points.Count-2]);
        points.Add(points[points.Count-1]+anchorPos*0.5f);
        points.Add(anchorPos);

        if(autoSetControlsPoints)
            AutoSetAllAffectedControlPoints(points.Count-1);//Last element on the list
    }

    public Vector2[] GetPointsInSegment(int i) 
    {
        return new Vector2[]
        {points[i*3], points[i*3+1], points[i*3+2], points[LoopIndex(i*3+3)]};
    }

    public void MovePoint(int i, Vector2 newPos)
    {
        Vector2 deltaMove = newPos - points[i];//How much our point moved

        if(i%3==0 || !autoSetControlsPoints)
        {
             points[i] = newPos;

            if(autoSetControlsPoints)
            {
                AutoSetAllAffectedControlPoints(i);
            }
            else//Manual move points
            {
                if(i%3==0)//Our point is an anchorPoint
                {
                    if(i+1<points.Count || isClosed)//last point does not exists
                    {
                        points[LoopIndex(i+1)] += deltaMove;
                    }
                    if(i-1>=0 || isClosed)//There is no point before the first one.
                    {
                        points[LoopIndex(i-1)] += deltaMove;
                    }
                }
                else
                {
                    bool nextPointIsAnchor = (i+1)%3 == 0;

                    int correspondingControlIndex = nextPointIsAnchor ? i+2 : i-2;
                    int anchorIndex = nextPointIsAnchor ? i+1 : i-1;

                    if(correspondingControlIndex >= 0 && correspondingControlIndex < points.Count || isClosed)
                    {
                        //Help to keep the distance between the acnhor point and the controls points
                        float distance = (points[LoopIndex(anchorIndex)]-points[LoopIndex(correspondingControlIndex)]).magnitude;
                        Vector2 direction = (points[LoopIndex(anchorIndex)]-newPos).normalized;
                        points[LoopIndex(correspondingControlIndex)] = points[LoopIndex(anchorIndex)]+direction * distance;
                    }
                }
            }
        }

    }


    void AutoSetAllAffectedControlPoints(int updatedAnchorPonit)
    {
        for(int i = updatedAnchorPonit-3; i <= updatedAnchorPonit+3; i+= 3)
        {
            if(i >= 0 && i < points.Count || isClosed)
            {
                AutoSetAnchorControlPoint(LoopIndex(i));
            }
        }

        AutoSetStartAndEndControls();
    }

    void AutoSetAllControlsPoints()
    {
        for(int i=0; i < points.Count;i+=3)
        {
            AutoSetAnchorControlPoint(i);
        }

        AutoSetStartAndEndControls();
    }
//The line made by the two control points of a anchor point, will be perpendicular to the line that is equally spaced from the two neighbour points.
    void AutoSetAnchorControlPoint(int anchorIndex)
    {
        Vector2 anchorPos = points[anchorIndex];
        Vector2 direction = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if(anchorIndex+3 >= 0 || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex+3)]-anchorPos;
            direction -= offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }

        if(anchorIndex-3 >= 0 || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex-3)]-anchorPos;
            direction += offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        direction.Normalize();

        for(int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex+i*2-1;
            if(controlIndex>=0 && controlIndex < points.Count || isClosed)
            {
                points[LoopIndex(controlIndex)] = anchorPos+direction * neighbourDistances[i]*0.5f;
            }
        }
    }

    //Used when the path is closed.
    int LoopIndex(int i)
    {
        return (i+points.Count)%points.Count;
    }

    //The fist control points will be between the second control point and his anchor point
    //The last control point will be between the 2 last control point and his anchor point
    void AutoSetStartAndEndControls()
    {
        if(!isClosed)
        {
            points[1] = (points[0]+points[2])*0.5f;
            points[points.Count-2] = (points[points.Count-1] + points[points.Count-3]) * 0.5f;
        }
    }

    public void SplitSegment(Vector2 anchorposition, int segmentIndex)
    {
        points.InsertRange(segmentIndex*3+2, new Vector2[]
        {
            Vector2.zero, anchorposition, Vector2.zero
        });

        if(autoSetControlsPoints)
        {
            AutoSetAllAffectedControlPoints(segmentIndex*3+3);
        }
        else
        {
            AutoSetAnchorControlPoint(segmentIndex*3+3);
        }
    }

    public void DeleteSegment(int anchorIndex)
    {
        if(NumSegments > 2 || !isClosed && NumSegments > 1)
        {
            if(anchorIndex == 0)
            {
                if(isClosed)
                {
                    points[points.Count-1] = points[2];
                }
                points.RemoveRange(0,3);
            }
            else if(anchorIndex == points.Count-1 && !isClosed)
            {
                points.RemoveRange(anchorIndex-2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex-1, 3);
            }
        }
    }
    Vector2[] p;
    float t = 0, overShootDistance, controlNetLenth, estimatedCurveLength, increment;
    Vector2 newEvenSpacedPoint;
    int divisions = 0;
    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution=1)
    {
        List<Vector2> evenSpacedPoints = new List<Vector2>();
        evenSpacedPoints.Add(points[0]);
        Vector2 previousPoint = points[0];
        float distanceSinceLastPoint = 0;

        for(int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            p = GetPointsInSegment(segmentIndex);
            controlNetLenth = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1],p[2]) + Vector2.Distance(p[2],p[3]);
            estimatedCurveLength = Vector2.Distance(p[0],p[3])+controlNetLenth*0.5f;
            divisions = Mathf.CeilToInt(estimatedCurveLength*resolution*10);
            t = 0;
            increment = 1f/divisions;
            while(t <= 1)
            {
                t += increment;
                Vector2 pointOnCurve = Bezier.EvaluateCubic(p[0],p[1],p[2],p[3],t);
                distanceSinceLastPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while(distanceSinceLastPoint >= spacing)//if the point goes beyond the right position, make it go back 
                {
                    overShootDistance = distanceSinceLastPoint - spacing;
                    newEvenSpacedPoint = pointOnCurve + (previousPoint-pointOnCurve).normalized * overShootDistance;
                    evenSpacedPoints.Add(newEvenSpacedPoint);
                    distanceSinceLastPoint = overShootDistance;
                    previousPoint = newEvenSpacedPoint;
                }

                previousPoint = pointOnCurve;
            }
            
        }

        return evenSpacedPoints.ToArray();
    }
}
