using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public static class Gesture_recog
{
    /// <summary>
    /// Compare two stroke point lists. Returns similarity 0..1 (1 = identical).
    /// </summary>
    /*    public static float Compare(List<Vector2> a, List<Vector2> b, int resampleCount)
        {
            if (a == null || b == null || a.Count < 2 || b.Count < 2) return 0f;

            // Normalize both
            List<Vector2> normA = NormalizePoints(a);
            List<Vector2> normB = NormalizePoints(b);

            // Compute distance
            float distance = HausdorffDistance(normA, normB);

            // Convert to similarity [0,1]
            float similarity = Mathf.Exp(-distance * 3f);  // exponential falloff
            return similarity;

            *//*// Use resampleCount here for preprocessing or normalization
            List<Vector2> resampledA = Resample(playerPath, resampleCount);
            List<Vector2> resampledB = Resample(truth, resampleCount);

            // Then calculate similarity (like DTW, Euclidean, etc.)
            float distance = CalculatePathDistance(resampledA, resampledB);
            return 1f - Mathf.Clamp01(distance); // example similarity score*//*
        }

        // ===================== NORMALIZATION =====================

        private static List<Vector2> NormalizePoints(List<Vector2> points)
        {
            // Remove duplicates
            List<Vector2> clean = points.Distinct().ToList();

            // Center around (0,0)
            Vector2 centroid = Vector2.zero;
            foreach (var p in clean) centroid += p;
            centroid /= clean.Count;
            for (int i = 0; i < clean.Count; i++) clean[i] -= centroid;

            // Scale to unit max distance
            float maxDist = clean.Max(p => p.magnitude);
            if (maxDist > 0f)
                for (int i = 0; i < clean.Count; i++) clean[i] /= maxDist;

            // Resample to fixed number of points
            clean = Resample(clean, 64);

            return clean;
        }

        // ===================== RESAMPLING =====================

        private static List<Vector2> Resample(List<Vector2> pts, int n)
        {
            if (pts.Count < 2) return pts;

            float totalLength = 0f;
            for (int i = 1; i < pts.Count; i++)
                totalLength += Vector2.Distance(pts[i - 1], pts[i]);

            float segmentLength = totalLength / (n - 1);
            List<Vector2> newPts = new List<Vector2> { pts[0] };
            float distSoFar = 0f;

            for (int i = 1; i < pts.Count; i++)
            {
                float d = Vector2.Distance(pts[i - 1], pts[i]);
                if (distSoFar + d >= segmentLength)
                {
                    float t = (segmentLength - distSoFar) / d;
                    Vector2 newPt = Vector2.Lerp(pts[i - 1], pts[i], t);
                    newPts.Add(newPt);
                    pts.Insert(i, newPt);
                    distSoFar = 0f;
                }
                else
                {
                    distSoFar += d;
                }
            }

            // Fill if we lost points due to rounding
            while (newPts.Count < n)
                newPts.Add(pts.Last());

            return newPts;
        }

        // ===================== HAUSDORFF DISTANCE =====================

        private static float HausdorffDistance(List<Vector2> a, List<Vector2> b)
        {
            float dAB = DirectedHausdorff(a, b);
            float dBA = DirectedHausdorff(b, a);
            return Mathf.Max(dAB, dBA);
        }

        private static float DirectedHausdorff(List<Vector2> a, List<Vector2> b)
        {
            float maxMinDist = 0f;
            foreach (var p in a)
            {
                float minDist = float.MaxValue;
                foreach (var q in b)
                {
                    float d = Vector2.Distance(p, q);
                    if (d < minDist) minDist = d;
                }
                if (minDist > maxMinDist) maxMinDist = minDist;
            }
            return maxMinDist;
        }*/

    public static float Compare(List<Vector2> path1, List<Vector2> path2, int sampleCount)
    {
        // Resample both paths to have the same number of points
        List<Vector2> sampled1 = ResamplePath(path1, sampleCount);
        List<Vector2> sampled2 = ResamplePath(path2, sampleCount);

        // Calculate total distance between corresponding points
        float totalDistance = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            totalDistance += Vector2.Distance(sampled1[i], sampled2[i]);
        }

        float averageDistance = totalDistance / sampleCount;

        // Convert distance to similarity (0-1)
        // Smaller distance = higher similarity
        float similarity = Mathf.Exp(-averageDistance * 5f);
        return Mathf.Clamp01(similarity);
    }

    private static List<Vector2> ResamplePath(List<Vector2> path, int targetCount)
    {
        if (path == null || path.Count < 2)
            return new List<Vector2>();

        List<Vector2> result = new List<Vector2>();

        // Calculate total path length
        float totalLength = 0f;
        for (int i = 1; i < path.Count; i++)
        {
            totalLength += Vector2.Distance(path[i - 1], path[i]);
        }

        float segmentLength = totalLength / (targetCount - 1);

        // Always include first point
        result.Add(path[0]);

        float currentLength = 0f;
        int currentSegment = 1;

        for (int i = 1; i < path.Count && result.Count < targetCount; i++)
        {
            float segmentDistance = Vector2.Distance(path[i - 1], path[i]);

            while (currentLength + segmentDistance >= segmentLength * currentSegment &&
                   result.Count < targetCount)
            {
                float t = (segmentLength * currentSegment - currentLength) / segmentDistance;
                Vector2 newPoint = Vector2.Lerp(path[i - 1], path[i], t);
                result.Add(newPoint);
                currentSegment++;
            }

            currentLength += segmentDistance;
        }

        // Ensure we have exactly targetCount points
        while (result.Count < targetCount)
        {
            result.Add(path[path.Count - 1]);
        }

        return result;
    }

}
