﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierPath
{
    // [SerializeField, HideInInspector]
    [SerializeField]
    List<Vector3> points;
    [SerializeField, HideInInspector]
    bool isClosed;
    [SerializeField, HideInInspector]
    bool autoSetControlPoints;

    public BezierPath(Vector3 center)
    {
        points = new List<Vector3>
        {
            center + Vector3.back, 
            center + (Vector3.back + Vector3.up) * 0.5f, 
            center + (Vector3.forward + Vector3.down) * 0.5f, 
            center + Vector3.forward
        };
    }

    public Vector3 this[int i]
    {
        get
        {
            return points[i];
        }
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
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                    points.Add(points[0] * 2 - points[1]);
                    if(autoSetControlPoints)
                    {
                        AutoSetAnchorControlPoints(0);
                        AutoSetAnchorControlPoints(points.Count - 3);
                    }
                }
                else
                {
                    points.RemoveRange(points.Count - 2, 2);
                    if(autoSetControlPoints)
                    {
                        AutoSetStartAndEndControls();
                    }
                }
            }
        }
    }

    public bool AutoSetControlPoints
    {
        get
        {
            return autoSetControlPoints;
        }
        set
        {
            if(autoSetControlPoints != value)
            {
                autoSetControlPoints = value;
                if(autoSetControlPoints)
                {
                    AutoSetAllControllPoits();
                }
            }
        }
    }

    public int NumPoits
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
            return points.Count / 3;
        }
    }

    public void AddSegment(Vector3 anchorPos)
    {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos) * 0.5f);
        points.Add(anchorPos);

        if(autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(points.Count - 1);
        }
    }

    public void SplitSegment(Vector3 anchorPos, int segmentIndex)
    {
        points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, anchorPos, Vector3.zero });
        if(autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3);
        }
        else
        {
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
        }
    }

    public void DeleteSegmnet(int anchorIndex)
    {
        if(NumSegments > 2 || !isClosed && NumSegments > 1)
        {
            if(anchorIndex == 0)
            {
                if(isClosed)
                {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            }
            else if(anchorIndex == points.Count - 1 && !isClosed)
            {
                points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new Vector3[]{points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[LoopIndex(i * 3 + 3)]};
    }

    public void UpdateLocalPosition()
    {

    }

    public void MovePoint(int i, Vector3 pos)
    {
        Vector3 deltaMove = pos - points[i];

        if(i % 3 == 0 || !autoSetControlPoints)
        {
            points[i] = pos;

            if(autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(i);
            }
            else
            {
                if(i % 3 == 0)
                {
                    if(i + 1 < points.Count || isClosed)
                        points[LoopIndex(i + 1)] += deltaMove;

                    if(i - 1 >= 0 || isClosed)
                        points[LoopIndex(i - 1)] += deltaMove;
                }
                else
                {
                    bool nextPointIsAnchor = (i + 1) % 3 == 0;
                    int correspomdingControlIndex = (nextPointIsAnchor) ? i + 2 : i - 2;
                    int anchorIndex = (nextPointIsAnchor) ? i + 1 : i - 1;

                    if(correspomdingControlIndex >= 0 && correspomdingControlIndex < points.Count || isClosed)
                    {
                        float dst = (points[LoopIndex(anchorIndex)] - points[LoopIndex(correspomdingControlIndex)]).magnitude;
                        Vector3 dir = (points[LoopIndex(anchorIndex)] - pos).normalized;
                        points[LoopIndex(correspomdingControlIndex)] = points[LoopIndex(anchorIndex)] + dir * dst;
                    }
                }
            }
        }
    }

    public Vector3[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        evenlySpacedPoints.Add(points[0]);
        Vector3 previusPoint = points[0];
        float dstSinceLastEvenPoint = 0;

        for(int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector3[] p = GetPointsInSegment(segmentIndex);
            float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2]) + Vector3.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector3.Distance(p[0], p[3]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while(t <= 1)
            {
                t += 1f / divisions;
                Vector3 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                dstSinceLastEvenPoint += Vector3.Distance(previusPoint, pointOnCurve);

                while(dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector3 newEvenlySpacedPoint = pointOnCurve + (previusPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previusPoint = newEvenlySpacedPoint;
                }
                previusPoint = pointOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();
    }

    void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
    {
        for(int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {
            if(i >= 0 && i < points.Count || isClosed)
            {
                AutoSetAnchorControlPoints(LoopIndex(i));
            }
        }

        AutoSetStartAndEndControls();
    }

    void AutoSetAllControllPoits()
    {
        for(int i = 0; i < points.Count; i += 3)
        {
            AutoSetAnchorControlPoints(i);
        }

        AutoSetStartAndEndControls();
    }

    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector3 anchorPos = points[anchorIndex];
        Vector3 dir = Vector3.zero;
        float[] neighbourDistance = new float[2];

        if(anchorIndex - 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistance[0] = offset.magnitude;
        }
        if(anchorIndex + 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistance[1] = -offset.magnitude;
        }

        dir.Normalize();

        for(int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if(controlIndex >= 0 && controlIndex < points.Count || isClosed)
            {
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistance[i] * 0.5f;
            }
        }
    }

    void AutoSetStartAndEndControls()
    {
        if(!isClosed)
        {
            points[1] = (points[0] + points[2]) * 0.5f;
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
        }
    }

    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }
}
