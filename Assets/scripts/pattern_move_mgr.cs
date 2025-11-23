using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class pattern_move_mgr : MonoBehaviour
{
    [System.Serializable]
    public class BallPattern
    {
        public ball_script ball;
        public PatternDefinition pattern;
        public float speed = 2f; // units per second
        [HideInInspector] public int currentTargetIndex = 0;
    }

    public List<BallPattern> ballPatterns;
    public Transform center;
    public List<string> patternNames = new List<string>();
    void Start()
    {

        PatternDefinition infinity = ScriptableObject.CreateInstance<PatternDefinition>();
        //PatternDefinition infinity = new PatternDefinition();
        infinity.patternName = "Infinity";
        patternNames.Add("Infinity");
        infinity.points = GenerateInfinityPoints(80, 1.5f);


        PatternDefinition mPattern = ScriptableObject.CreateInstance<PatternDefinition>();

        //PatternDefinition mPattern = new PatternDefinition();
        mPattern.patternName = "MShape";
        patternNames.Add("MShape");
        mPattern.points = GenerateMPoints(80, 2f, 1.5f);

        PatternDefinition triangle = ScriptableObject.CreateInstance<PatternDefinition>();

        //PatternDefinition triangle = new PatternDefinition();
        triangle.patternName = "Triangle";
        patternNames.Add("Triangle");
        triangle.points.Add(new Vector2(0, 1));
        triangle.points.Add(new Vector2(Mathf.Sqrt(3) / 2, -0.5f));
        triangle.points.Add(new Vector2(-Mathf.Sqrt(3) / 2, -0.5f));

        PatternDefinition circle = ScriptableObject.CreateInstance<PatternDefinition>();
        circle.patternName = "Circle";
        patternNames.Add("Circle");
        circle.points = GenerateCircle(20, 0.8f);

        PatternDefinition square = ScriptableObject.CreateInstance<PatternDefinition>();
        square.patternName = "Square";
        patternNames.Add("Square");
        square.points = GenerateSquare(1f);

        PatternDefinition polygon = ScriptableObject.CreateInstance<PatternDefinition>();
        polygon.patternName = "Polygon";
        patternNames.Add("Polygon");
        polygon.points = GeneratePolygon(5, 1f);


        /*        PatternDefinition infinity = new PatternDefinition();
                infinity.patternName = "Infinity";
                infinity.points.Add(new Vector2(-1, 0));
                infinity.points.Add(new Vector2(0, 1));
                infinity.points.Add(new Vector2(1, 0));
                infinity.points.Add(new Vector2(0, -1));*/

        // Create pattern list in order
        List<PatternDefinition> patterns = new List<PatternDefinition>()
        {
             infinity,
             mPattern,
             triangle,
             circle,
             square,
             polygon,
             
             
             
        };

        // Assign patterns safely to available balls
        for (int i = 0; i < ballPatterns.Count; i++)
        {
            // Use modulo so patterns repeat if balls > patterns
            ballPatterns[i].pattern = patterns[i % patterns.Count];
        }
        /*ballPatterns[0].pattern =  circle;
        ballPatterns[1].pattern = square;
        ballPatterns[2].pattern = polygon;
        ballPatterns[3].pattern = triangle;
        ballPatterns[4].pattern = infinity;
        ballPatterns[5].pattern = mPattern;*/
        // Center all patterns around a point (usually screen center)
        foreach (var bp in ballPatterns)
        {
            if (bp.pattern != null && bp.pattern.points.Count > 0)
            {
                Vector2 startPos = center.position + (Vector3)(bp.pattern.points[0] * bp.pattern.scale);
                bp.ball.SetPosition(startPos);
            }
        }
    }

    void Update()
    {
        foreach (var bp in ballPatterns)
        {
            MoveBallAlongPattern(bp);
        }
    }

    void MoveBallAlongPattern(BallPattern bp)
    {
        if (bp.pattern == null || bp.pattern.points.Count == 0) return;

        // Get current and next target points
        Vector2 currentPos = bp.ball.transform.position;
        Vector2 target = center.position + (Vector3)(bp.pattern.points[bp.currentTargetIndex] * bp.pattern.scale);

        float step = bp.speed * Time.deltaTime;
        bp.ball.transform.position = Vector2.MoveTowards(currentPos, target, step);

        // Check if reached target
        if (Vector2.Distance(bp.ball.transform.position, target) < 0.01f)
        {
            bp.currentTargetIndex++;
            if (bp.currentTargetIndex >= bp.pattern.points.Count)
                bp.currentTargetIndex = 0; // loop path
        }
    }



    public static List<Vector2> GenerateInfinityPoints(int segments = 60, float radius = 1.5f)
    {
        List<Vector2> pts = new List<Vector2>();
        for (int i = 0; i < segments; i++)
        {
            float t = Mathf.Lerp(-Mathf.PI / 2, Mathf.PI / 2, i / (float)(segments - 1));
            float x = radius * Mathf.Sin(t);
            float y = radius * Mathf.Sin(t) * Mathf.Cos(t);
            pts.Add(new Vector2(x, y));
        }
        // Mirror for the other loop
        for (int i = 0; i < segments; i++)
        {
            float t = Mathf.Lerp(Mathf.PI / 2, 3 * Mathf.PI / 2, i / (float)(segments - 1));
            float x = radius * Mathf.Sin(t);
            float y = radius * Mathf.Sin(t) * Mathf.Cos(t);
            pts.Add(new Vector2(x, y));
        }
        return pts;
    }


    public static List<Vector2> GenerateMPoints(int segments = 100, float width = 2f, float height = 1.5f)
    {
        List<Vector2> pts = new List<Vector2>();

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float x = Mathf.Lerp(-width / 2f, width / 2f, t);
            // Create an M with sine-based curvature
            float y = Mathf.Abs(Mathf.Sin(2 * Mathf.PI * t)) * height - height / 2f;
            pts.Add(new Vector2(x, y));
        }

        return pts;
    }

    public static List<Vector2> GenerateCircle(int segments, float radius)
    {
        List<Vector2> pts = new List<Vector2>();
        for (int i = 0; i < segments; i++)
        {
            float a = 2f * Mathf.PI * (i / (float)segments);
            pts.Add(new Vector2(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius));
        }
        return pts;
    }

    public static List<Vector2> GenerateSquare(float size)
    {
        return new List<Vector2>()
    {
        new Vector2(-size,  size),
        new Vector2( size,  size),
        new Vector2( size, -size),
        new Vector2(-size, -size)
    };
    }
    public static List<Vector2> GeneratePolygon(int sides, float radius)
    {
        List<Vector2> pts = new List<Vector2>();

        for (int i = 0; i < sides; i++)
        {
            float angle = 2f * Mathf.PI * (i / (float)sides);
            pts.Add(new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
        }

        return pts;
    }




    public List<Vector2> SamplePatternScreenNormalized(BallPattern bp, int sampleCount = 128)
    {
        var pts = new List<Vector2>();
        if (bp == null || bp.pattern == null || bp.pattern.points == null || bp.pattern.points.Count == 0) return pts;

        // Build a polyline that visits bp.pattern.points in order and then loops
        List<Vector2> poly = new List<Vector2>(bp.pattern.points);
        // optionally close loop by adding first point at end
        if (poly.Count > 1) poly.Add(poly[0]);

        // sample along each segment proportionally to its length
        float total = 0f;
        float[] segLen = new float[poly.Count - 1];
        for (int i = 0; i < poly.Count - 1; i++)
        {
            segLen[i] = Vector2.Distance(poly[i], poly[i + 1]);
            total += segLen[i];
        }
        if (total <= Mathf.Epsilon)
        {
            // fallback: single point repeated
            for (int i = 0; i < sampleCount; i++) pts.Add(new Vector2(0.5f, 0.5f));
            return pts;
        }

        // walk segments and sample
        float step = total / (sampleCount - 1);
        float acc = 0f;
        int segIndex = 0;
        float segAcc = 0f;
        Vector2 cur = poly[0];

        for (int s = 0; s < sampleCount; s++)
        {
            float target = s * step;
            // advance segments until target fits
            while (segIndex < segLen.Length && acc + segLen[segIndex] < target)
            {
                acc += segLen[segIndex];
                segIndex++;
            }
            if (segIndex >= segLen.Length)
            {
                cur = poly[poly.Count - 1];
            }
            else
            {
                float local = (target - acc) / Mathf.Max(1e-6f, segLen[segIndex]);
                Vector2 p = Vector2.Lerp(poly[segIndex], poly[segIndex + 1], local);
                cur = p;
            }

            // convert cur (pattern space) -> world -> screen -> normalized
            Vector3 world = center.position + new Vector3(cur.x * bp.pattern.scale, 0f, cur.y * bp.pattern.scale);
            Vector3 screen = Camera.main.WorldToScreenPoint(world);
            Vector2 norm = new Vector2(screen.x / Screen.width, screen.y / Screen.height);
            pts.Add(norm);
        }

        return pts;
    }

}
