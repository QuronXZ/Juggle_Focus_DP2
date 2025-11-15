using UnityEngine;

// Utils_Path.cs
using System.Collections.Generic;

public static class Utils_Path
{
    // Resample a path (list of Vector2 normalized coords) to N points using linear interpolation
    public static List<Vector2> Resample(List<Vector2> pts, int n)
    {
        List<Vector2> res = new List<Vector2>();
        if (pts == null || pts.Count == 0) return res;
        if (pts.Count == 1)
        {
            for (int i = 0; i < n; i++) res.Add(pts[0]);
            return res;
        }

        // compute segment lengths
        float total = 0f;
        float[] seg = new float[pts.Count - 1];
        for (int i = 0; i < pts.Count - 1; i++)
        {
            seg[i] = Vector2.Distance(pts[i], pts[i + 1]);
            total += seg[i];
        }

        if (total == 0f)
        {
            for (int i = 0; i < n; i++) res.Add(pts[0]);
            return res;
        }

        float step = total / (n - 1);
        float distSoFar = 0f;
        int curIdx = 0;
        res.Add(pts[0]);

        for (int i = 1; i < n - 1; i++)
        {
            float target = step * i;
            while (curIdx < seg.Length && distSoFar + seg[curIdx] < target)
            {
                distSoFar += seg[curIdx];
                curIdx++;
            }
            float remain = target - distSoFar;
            float frac = (seg[curIdx] > 0f) ? remain / seg[curIdx] : 0f;
            Vector2 p = Vector2.Lerp(pts[curIdx], pts[curIdx + 1], frac);
            res.Add(p);
        }
        res.Add(pts[pts.Count - 1]);
        return res;
    }

    // Translate to origin and scale to unit bounding box
    public static List<Vector2> NormalizePath(List<Vector2> pts)
    {
        List<Vector2> copy = new List<Vector2>(pts);
        if (copy.Count == 0) return copy;

        // centroid
        Vector2 c = Vector2.zero;
        foreach (var p in copy) c += p;
        c /= copy.Count;
        for (int i = 0; i < copy.Count; i++) copy[i] -= c;

        // scale to max abs coordinate = 1
        float maxAbs = 0f;
        foreach (var p in copy)
        {
            maxAbs = Mathf.Max(maxAbs, Mathf.Abs(p.x), Mathf.Abs(p.y));
        }
        if (maxAbs > 0f)
        {
            for (int i = 0; i < copy.Count; i++) copy[i] /= maxAbs;
        }
        return copy;
    }

    public static float AveragePointDistance(List<Vector2> a, List<Vector2> b)
    {
        if (a.Count != b.Count) return float.MaxValue;
        float sum = 0f;
        for (int i = 0; i < a.Count; i++) sum += Vector2.Distance(a[i], b[i]);
        return sum / a.Count;
    }
}
