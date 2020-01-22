using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    public static PathCreator instance { get { return s_Instance; } }
    protected static PathCreator s_Instance;

    // [HideInInspector]
    public BezierPath path;

    public Color anchorCol = Color.red;
    public Color controlCol = Color.white;
    public Color segmentCol = Color.green;
    public Color selectedSegmentCol = Color.yellow;
    public float anchorDiameter = 0.1f;
    public float controlDiameter = 0.075f;
    public bool displayControlPoints = true;

    void Awake()
    {
        s_Instance = this;
    }

    public void CreatePath()
    {
        path = new BezierPath(transform.position);

        // int length = Random.Range(5, 10);

        path.MovePoint(0, new Vector3(5, 0, -5));
        path.MovePoint(3, new Vector3(8, 0, 0));

        for(int i = 0; i < 6; i++)
        {
            path.AddSegment(GetPosition(i));
        }

        path.IsClosed = true;
        path.AutoSetControlPoints = true;
    }

    Vector3 GetPosition(int i)
    {
        Vector3 vector = Vector3.zero;

        switch(i)
        {
            case 0:
                vector = new Vector3(5, 0, 5);
                break;
            case 1:
                vector = new Vector3(2.5f, 0, 7);
                break;
            case 2:
                vector = new Vector3(0, 0, 10);
                break;
            case 3:
                vector = new Vector3(-2.75f, 0, 5.25f);
                break;
            case 4:
                vector = new Vector3(-5.5f, 0, -2.5f);
                break;
            case 5:
                vector = new Vector3(0, 0, -5);
                break;
        }

        return vector;
    }

    void Reset()
    {
        CreatePath();
    }
}
