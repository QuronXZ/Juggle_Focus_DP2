using System.Collections.Generic;
using UnityEngine;

public static class PatternComparer
{
    // resampleCount sets resolution of comparison
    public static float ComparePaths(List<Vector2> drawnNorm, List<Vector2> truthNorm, int resampleCount = 64, float tolerance = 0.8f)
    {
        if (drawnNorm == null || drawnNorm.Count < 2 || truthNorm == null || truthNorm.Count < 2) return 0f;

        // resample both to same number of points
        var d = Utils_Path.Resample(drawnNorm, resampleCount);
        var t = Utils_Path.Resample(truthNorm, resampleCount);

        // normalize (centroid + scale)
        d = Utils_Path.NormalizePath(d);
        t = Utils_Path.NormalizePath(t);

        // compute average point distance
        float avg = Utils_Path.AveragePointDistance(d, t); // in normalized units (~0..2)
        // convert avg to match percent: smaller avg -> higher percent
        // choose a scaling factor: if avg==0 => 1, if avg >= 1 => 0
        float match = Mathf.Clamp01(1f - avg / tolerance);
        return match;
    }
}
