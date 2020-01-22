using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StructExtension
{
    public static Vector2 With(this Vector2 orig, float? x = null, float? y = null)
    {
        if (x.HasValue) orig.x = x.Value;
        if (y.HasValue) orig.y = y.Value;
        return orig;
    }

    public static Vector2 WithAdding(this Vector2 orig, float? x = null, float? y = null)
    {
        if (x.HasValue) orig.x += x.Value;
        if (y.HasValue) orig.y += y.Value;
        return orig;
    }

    public static Vector3 With(this Vector3 orig, float? x = null, float? y = null, float? z = null)
    {
        if (x.HasValue) orig.x = x.Value;
        if (y.HasValue) orig.y = y.Value;
        if (z.HasValue) orig.z = z.Value;
        return orig;
    }

    public static Vector3 WithAdding(this Vector3 orig, float? x = null, float? y = null, float? z = null)
    {
        if (x.HasValue) orig.x += x.Value;
        if (y.HasValue) orig.y += y.Value;
        if (z.HasValue) orig.z += z.Value;
        return orig;
    }

    public static Quaternion With(this Quaternion orig, float x, float y, float z)
    {
        orig = Quaternion.Euler(x, y, z);

        return orig;
    }

    public static Quaternion WithY(this Quaternion orig, float y = 0)
    {
        orig = Quaternion.AngleAxis(y, Vector3.up);
        return orig;
    }

    public static Quaternion WithZ(this Quaternion orig, float z = 0)
    {
        orig = Quaternion.AngleAxis(z, Vector3.forward);
        return orig;
    }

    public static Color With(this Color orig, float? r = null, float? g = null, float? b = null, float? a = null)
    {
        if (r.HasValue) orig.r = r.Value;
        if (g.HasValue) orig.g = g.Value;
        if (b.HasValue) orig.b = b.Value;
        if (a.HasValue) orig.a = a.Value;
        return orig;
    }
}
