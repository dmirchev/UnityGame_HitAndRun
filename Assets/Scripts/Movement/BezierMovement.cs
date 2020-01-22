using UnityEngine;

public class BezierMovement : MonoBehaviour
{
    public static BezierMovement instance { get { return s_Instance; } }
    protected static BezierMovement s_Instance;

    public Vector3[] points;

    void Awake()
    {
        s_Instance = this;
    }

    public void SetPoints(Vector3[] evenPoints)
    {
        points = evenPoints;
    }

    public Vector3 GetDirection(int index)
    {
        Vector3 direction = Vector3.zero;
        direction += points[(index + 1) % points.Length] - points[index];
        direction += points[index] - points[(index - 1 + points.Length) % points.Length];
        direction.Normalize();

        return direction;
    }

    public Vector3 CalculateLerp(int index0, int index1, float increment)
    {
        Vector3 lerp = Vector3.LerpUnclamped(
            points[index0], 
            points[index1], 
            increment
        );

        return lerp;
    }
}
