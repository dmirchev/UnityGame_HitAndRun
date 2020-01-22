using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadCreator : MonoBehaviour
{
    public static RoadCreator instance { get { return s_Instance; } }
    protected static RoadCreator s_Instance;

    [Range(0.05f, 1.5f)]
    public float spacing = 1;
    public float roadWidth = 1;
    public bool autoUpdate;
    public float tiling = 1;

    public Vector3[] evenPoints;

    void Awake()
    {
        s_Instance = this;
    }

    void Start()
    {
        BezierPath path = GetComponent<PathCreator>().path;
        evenPoints = path.CalculateEvenlySpacedPoints(spacing * 2);

        BezierMovement.instance.SetPoints(evenPoints);
    }

    public void UpdateRoad()
    {
        // BezierPath path = GetComponent<PathCreator>().path;
        // Vector3[] points = path.CalculateEvenlySpacedPoints(spacing * 2);
        // // GetComponent<MeshFilter>().mesh = CreateRoadMesh(points, path.IsClosed);
        // GetComponent<MeshFilter>().mesh = CreateRoadMeshWithBottom(points, path.IsClosed);

        // int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * 0.05f);
        // GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
    }

    Mesh CreateRoadMesh(Vector3[] points, bool isClosed)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];

        int numTris = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for(int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if(i < points.Length - 1 || isClosed)
            {
                forward += points[(i + 1) % points.Length] - points[i];
            }
            if(i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }
            forward.Normalize();
            // Vector2 left = new Vector2(-forward.y, forward.x);
            Vector3 left = new Vector3(-forward.z, 0, forward.x);
            // Debug.DrawRay(points[i], forward);

            verts[vertIndex] = points[i] + left * roadWidth * 0.5f;
            verts[vertIndex + 1] = points[i] - left * roadWidth * 0.5f;

            Debug.DrawRay(verts[vertIndex], Vector3.down, Color.cyan);
            Debug.DrawRay(verts[vertIndex + 1], Vector3.down, Color.cyan);

            float completionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector2(0, v);
            uvs[vertIndex + 1] = new Vector2(1, v);

            if(i < points.Length - 1 || isClosed)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }

    Mesh CreateRoadMeshWithBottom(Vector3[] points, bool isClosed)
    {
        Vector3[] verts = new Vector3[points.Length * 2 * 2];
        Vector2[] uvs = new Vector2[verts.Length];

        int numTris = 3 * 2 * (points.Length - 1) + ((isClosed) ? 6 : 0);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for(int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if(i < points.Length - 1 || isClosed)
            {
                forward += points[(i + 1) % points.Length] - points[i];
            }
            if(i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }
            forward.Normalize();
            // Vector2 left = new Vector2(-forward.y, forward.x);
            Vector3 left = new Vector3(-forward.z, 0, forward.x);
            // Debug.DrawRay(points[i], forward);

            verts[vertIndex] = points[i].With(y: -20) + left * roadWidth * 0.5f;
            verts[vertIndex + 1] = points[i] + left * roadWidth * 0.5f;
            verts[vertIndex + 2] = points[i] - left * roadWidth * 0.5f;
            verts[vertIndex + 3] = points[i].With(y: -20) - left * roadWidth * 0.5f;

            // Debug.DrawRay(verts[vertIndex], Vector3.down, Color.cyan);
            // Debug.DrawRay(verts[vertIndex + 1], Vector3.down, Color.cyan);
            // Debug.DrawRay(verts[vertIndex + 2], Vector3.down, Color.cyan);
            // Debug.DrawRay(verts[vertIndex + 3], Vector3.down, Color.cyan);

            float completionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector2(0, 0);
            uvs[vertIndex + 1] = new Vector2(0.25f, v);
            uvs[vertIndex + 2] = new Vector2(0.75f, v);
            uvs[vertIndex + 3] = new Vector2(1, 1);
            // uvs[vertIndex] = new Vector2(0.25f, 0);
            // uvs[vertIndex + 1] = new Vector2(1, v);
            // uvs[vertIndex + 2] = new Vector2(0, v);
            // uvs[vertIndex + 3] = new Vector2(0.25f, 1);

            if(i < points.Length - 1 || isClosed)
            {
                // Inner
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 4) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1; // 1
                tris[triIndex + 4] = (vertIndex + 4) % verts.Length; // 2
                tris[triIndex + 5] = (vertIndex + 5) % verts.Length; // 3

                // Top
                tris[triIndex + 6] = vertIndex + 1;
                tris[triIndex + 7] = (vertIndex + 4 + 1) % verts.Length;
                tris[triIndex + 8] = vertIndex + 1 + 1;

                tris[triIndex + 9] = vertIndex + 1 + 1; // 1
                tris[triIndex + 10] = (vertIndex + 4 + 1) % verts.Length; // 2
                tris[triIndex + 11] = (vertIndex + 5 + 1) % verts.Length; // 3

                // Outter
                tris[triIndex + 12] = vertIndex + 1 + 1;
                tris[triIndex + 13] = (vertIndex + 4 + 1 + 1) % verts.Length;
                tris[triIndex + 14] = vertIndex + 1 + 1 + 1;

                tris[triIndex + 15] = vertIndex + 1 + 1 + 1; // 1
                tris[triIndex + 16] = (vertIndex + 4 + 1 + 1) % verts.Length; // 2
                tris[triIndex + 17] = (vertIndex + 5 + 1 + 1) % verts.Length; // 3

                // tris[triIndex + 3] = 3;
                // tris[triIndex + 4] = 5;
                // tris[triIndex + 5] = 6;

                // tris[triIndex] = vertIndex; // 0
                // tris[triIndex + 1] = (vertIndex + 2) % verts.Length; // 2
                // tris[triIndex + 2] = vertIndex + 1; // 1

                // tris[triIndex + 3] = vertIndex + 1; // 1
                // tris[triIndex + 4] = (vertIndex + 2) % verts.Length; // 2
                // tris[triIndex + 5] = (vertIndex + 3) % verts.Length; // 3

                // tris[triIndex + 6] = vertIndex + 4;
                // tris[triIndex + 7] = (vertIndex + 5) % verts.Length;
                // tris[triIndex + 8] = (vertIndex + 6) % verts.Length;

                // tris[triIndex + 9] = vertIndex + 7;
                // tris[triIndex + 10] = (vertIndex + 8) % verts.Length;
                // tris[triIndex + 11] = (vertIndex + 9) % verts.Length;
            }

            vertIndex += 4;
            triIndex += 18;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }

    public Mesh CreateObstacle(int startIndex, int obstacleLength, bool side)
    {
        int count = evenPoints.Length - 10;

        Vector3[] obstaclePoints = new Vector3[obstacleLength];

        for(int i = 0; i < obstacleLength; i++)
        {
            int position = (startIndex + i) % count;
            obstaclePoints[i] = evenPoints[position];
        }

        Mesh mesh = CreateObstacleMesh(obstaclePoints, side, evenPoints.Length);

        return mesh;
    }

    public Mesh CreateObstacleMesh(Vector3[] points, bool side, int realPointsLenght)
    {
        // Insert a one Line of Verts to make a UV Split
        // Number of Points * How many Verts (2 top and 2 Bottom) + Split + Start + End
        Vector3[] verts = new Vector3[points.Length * 2 * 2 + (points.Length) + (4 * 2)];
        Vector2[] uvs = new Vector2[verts.Length];

        int numTris = 6 * points.Length;
        // Two Sides for Start And End
        int[] tris = new int[(numTris * 3) + (6 * 2)];
        int vertIndex = 0;
        int triIndex = 0;

        // Debug.Log("Verts: " + verts.Length);
        // Debug.Log("UVs: " + uvs.Length);
        // Debug.Log("Tris: " + tris.Length);
        int vertsCount = verts.Length;
        // side = true;

        if(side)
        {
            for(int i = 0; i < points.Length; i++)
            {
                Vector3 forward = Vector3.zero;
                if(i < points.Length - 1)
                {
                    forward += points[(i + 1) % points.Length] - points[i];
                }
                if(i > 0)
                {
                    forward += points[i] - points[(i - 1 + points.Length) % points.Length];
                }
                forward.Normalize();
                Vector3 left = new Vector3(-forward.z, 0, forward.x);

                // Spit
                int splitOffset = 1;
                // Debug.Log("Position: " + i + " - " + points[i]);

                verts[vertIndex] = points[i].With(y: -5);
                verts[vertIndex + 1] = points[i];
                verts[vertIndex + 1 + splitOffset] = points[i];
                verts[vertIndex + 2 + splitOffset] = points[i] - left * roadWidth * 0.5f;
                verts[vertIndex + 3 + splitOffset] = points[i].With(y: -5) - left * roadWidth * 0.5f;

                // Start Side
                if(i == 0)
                {
                    verts[vertsCount - 1] = points[i].With(y: -5);
                    verts[vertsCount - 2] = points[i];
                    verts[vertsCount - 3] = points[i] - left * roadWidth * 0.5f;
                    verts[vertsCount - 4] = points[i].With(y: -5) - left * roadWidth * 0.5f;
                }
                // End Side
                if(i == points.Length - 1)
                {
                    verts[vertsCount - 5] = points[i].With(y: -5);
                    verts[vertsCount - 6] = points[i];
                    verts[vertsCount - 7] = points[i] - left * roadWidth * 0.5f;
                    verts[vertsCount - 8] = points[i].With(y: -5) - left * roadWidth * 0.5f;
                }

                // Debug.Log("------------------");
                // Debug.Log("Vert: " + verts[vertIndex + 0]);
                // Debug.Log("Vert: " + verts[vertIndex + 1]);
                // Debug.Log("Vert: " + verts[vertIndex + 2]);
                // Debug.Log("Vert: " + verts[vertIndex + 3]);
                // Debug.Log("Vert: " + verts[vertIndex + 4]);
                // Debug.Log("Point: " + points[i]);

                // float completionPercent = i / (float)(points.Length - 1);
                float completionPercent = i / (float)(realPointsLenght - 1);
                float v = 1 - Mathf.Abs(2 * completionPercent - 1);

                uvs[vertIndex] = new Vector2(0, 0);
                uvs[vertIndex + 1] = new Vector2(0.25f, v);
                uvs[vertIndex + 2] = new Vector2(0.5f, v);
                uvs[vertIndex + 3] = new Vector2(0.75f, v);
                uvs[vertIndex + 4] = new Vector2(1, 1);

                if(i < points.Length - 1)
                {
                    // Inner
                    tris[triIndex] = vertIndex;
                    tris[triIndex + 1] = (vertIndex + 4 + splitOffset) % verts.Length;
                    tris[triIndex + 2] = vertIndex + 1;

                    tris[triIndex + 3] = vertIndex + 1; // 1
                    tris[triIndex + 4] = (vertIndex + 4 + splitOffset) % verts.Length; // 2
                    tris[triIndex + 5] = (vertIndex + 5 + splitOffset) % verts.Length; // 3

                    // Top
                    tris[triIndex + 6] = vertIndex + 1 + splitOffset;
                    tris[triIndex + 7] = (vertIndex + 4 + 1 + 1 + splitOffset) % verts.Length;
                    tris[triIndex + 8] = vertIndex + 1 + 1 + splitOffset;

                    tris[triIndex + 9] = vertIndex + 1 + 1 + splitOffset; // 1
                    tris[triIndex + 10] = (vertIndex + 4 + 1 + splitOffset + 1 ) % verts.Length; // 2
                    tris[triIndex + 11] = (vertIndex + 5 + 1 + splitOffset + 1 ) % verts.Length; // 3

                    // Outter
                    tris[triIndex + 12] = vertIndex + 1 + 1 + splitOffset;
                    tris[triIndex + 13] = (vertIndex + 4 + 1 + 1 + splitOffset + 1) % verts.Length;
                    tris[triIndex + 14] = vertIndex + 1 + 1 + 1 + splitOffset;

                    tris[triIndex + 15] = vertIndex + 1 + 1 + 1 + splitOffset; // 1
                    tris[triIndex + 16] = (vertIndex + 4 + 1 + 1 + splitOffset + 1) % verts.Length; // 2
                    tris[triIndex + 17] = (vertIndex + 5 + 1 + 1 + splitOffset + 1) % verts.Length; // 3
                }

                vertIndex += 4 + splitOffset;
                triIndex += 18;
            }
        }
        else
        {
            for(int i = 0; i < points.Length; i++)
            {
                Vector3 forward = Vector3.zero;
                if(i < points.Length - 1)
                {
                    forward += points[(i + 1) % points.Length] - points[i];
                }
                if(i > 0)
                {
                    forward += points[i] - points[(i - 1 + points.Length) % points.Length];
                }
                forward.Normalize();
                Vector3 left = new Vector3(-forward.z, 0, forward.x);

                // Debug.Log("Side: " + side);

                // Spit
                int splitOffset = 1;
                // Debug.Log("Position: " + i + " - " + points[i]);

                verts[vertIndex] = points[i].With(y: -5) + left * roadWidth * 0.5f;
                verts[vertIndex + 1] = points[i] + left * roadWidth * 0.5f;
                verts[vertIndex + 1 + splitOffset] = points[i];
                verts[vertIndex + 2 + splitOffset] = points[i];
                verts[vertIndex + 3 + splitOffset] = points[i].With(y: -5);

                // Start Side
                if(i == 0)
                {
                    verts[vertsCount - 1] = points[i].With(y: -5) + left * roadWidth * 0.5f;
                    verts[vertsCount - 2] = points[i] + left * roadWidth * 0.5f;
                    verts[vertsCount - 3] = points[i];
                    verts[vertsCount - 4] = points[i].With(y: -5);
                }
                // End Side
                if(i == points.Length - 1)
                {
                    verts[vertsCount - 5] = points[i].With(y: -5) + left * roadWidth * 0.5f;
                    verts[vertsCount - 6] = points[i] + left * roadWidth * 0.5f;
                    verts[vertsCount - 7] = points[i];
                    verts[vertsCount - 8] = points[i].With(y: -5);
                }

                // float completionPercent = i / (float)(points.Length - 1);
                float completionPercent = i / (float)(realPointsLenght - 1);
                float v = 1 - Mathf.Abs(2 * completionPercent - 1);

                uvs[vertIndex] = new Vector2(1, 1);
                uvs[vertIndex + 1] = new Vector2(0.75f, v);
                uvs[vertIndex + 2] = new Vector2(0.5f, v);
                uvs[vertIndex + 3] = new Vector2(0.25f, v);
                uvs[vertIndex + 4] = new Vector2(0, 0);

                if(i < points.Length - 1)
                {
                    // Inner
                    tris[triIndex] = vertIndex;
                    tris[triIndex + 1] = (vertIndex + 4 + splitOffset) % verts.Length;
                    tris[triIndex + 2] = vertIndex + 1;

                    tris[triIndex + 3] = vertIndex + 1; // 1
                    tris[triIndex + 4] = (vertIndex + 4 + splitOffset) % verts.Length; // 2
                    tris[triIndex + 5] = (vertIndex + 5 + splitOffset) % verts.Length; // 3

                    // Top
                    tris[triIndex + 6] = vertIndex + 1;
                    tris[triIndex + 7] = (vertIndex + 4 + 1 + splitOffset) % verts.Length;
                    tris[triIndex + 8] = vertIndex + 1 + 1;

                    tris[triIndex + 9] = vertIndex + 1 + 1; // 1
                    tris[triIndex + 10] = (vertIndex + 4 + 1 + splitOffset) % verts.Length; // 2
                    tris[triIndex + 11] = (vertIndex + 5 + 1 + splitOffset) % verts.Length; // 3

                    // Outter
                    tris[triIndex + 12] = vertIndex + 1 + 1 + splitOffset;
                    tris[triIndex + 13] = (vertIndex + 4 + 1 + 1 + splitOffset + 1) % verts.Length;
                    tris[triIndex + 14] = vertIndex + 1 + 1 + 1 + splitOffset;

                    tris[triIndex + 15] = vertIndex + 1 + 1 + 1 + splitOffset; // 1
                    tris[triIndex + 16] = (vertIndex + 4 + 1 + 1 + splitOffset + 1) % verts.Length; // 2
                    tris[triIndex + 17] = (vertIndex + 5 + 1 + 1 + splitOffset + 1) % verts.Length; // 3
                }

                vertIndex += 4 + splitOffset;
                triIndex += 18;
            }
        }

        // Set Start And End Faces
        // Start
        uvs[vertsCount - 1] = new Vector2(0, 0.25f);
        uvs[vertsCount - 2] = new Vector2(0, 0.25f);
        uvs[vertsCount - 3] = new Vector2(0.25f, 0.25f);
        uvs[vertsCount - 4] = new Vector2(0.25f, 0.25f);

        tris[triIndex] = vertsCount - 1;
        tris[triIndex + 1] = vertsCount - 2;
        tris[triIndex + 2] = vertsCount - 3;

        tris[triIndex + 3] = vertsCount - 3;
        tris[triIndex + 4] = vertsCount - 4;
        tris[triIndex + 5] = vertsCount - 1;

        // End
        uvs[vertsCount - 5] = new Vector2(0, 0.25f);
        uvs[vertsCount - 6] = new Vector2(0, 0.25f);
        uvs[vertsCount - 7] = new Vector2(0.25f, 0.25f);
        uvs[vertsCount - 8] = new Vector2(0.25f, 0.25f);

        tris[triIndex + 6] = vertsCount - 5;
        tris[triIndex + 7] = vertsCount - 7;
        tris[triIndex + 8] = vertsCount - 6;

        tris[triIndex + 9] = vertsCount - 7;
        tris[triIndex + 10] = vertsCount - 5;
        tris[triIndex + 11] = vertsCount - 8;

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }
}
