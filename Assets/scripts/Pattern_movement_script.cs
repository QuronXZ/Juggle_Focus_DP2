using UnityEngine;
using System.Collections.Generic;

public class Pattern_movement_script : MonoBehaviour

{
    [System.Serializable]
    public class BallPatternData
    {
        public GameObject ballObject;
        public PatternType patternType;
        public float movementSpeed;
        public float phaseOffset;
        public float radius;
        public float currentAngle;
        public bool isActive = true;
    }

    public enum PatternType
    {
        Circle, Infinity, Wave, Triangle,
        Square, Figure8, Lissajous, Spiral
    }

    [Header("Pattern System Settings")]
    public List<BallPatternData> allBallsData = new List<BallPatternData>();
    public bool isMoving = true;
    public float globalSpeedMultiplier = 1f;

    [Header("Pattern Parameters")]
    public float baseRadius = 3f;

    void Update()
    {
        if (!isMoving) return;

        foreach (BallPatternData ballData in allBallsData)
        {
            if (ballData.ballObject != null && ballData.isActive)
            {
                UpdateBallPosition(ballData);
            }
        }
    }

    private void UpdateBallPosition(BallPatternData ballData)
    {
        ballData.currentAngle += Time.deltaTime * ballData.movementSpeed * globalSpeedMultiplier;
        Vector3 newPosition = CalculatePatternPosition(ballData);
        ballData.ballObject.transform.position = newPosition;
    }

    private Vector3 CalculatePatternPosition(BallPatternData ballData)
    {
        float angle = ballData.currentAngle + ballData.phaseOffset;
        float radius = ballData.radius;

        switch (ballData.patternType)
        {
            case PatternType.Circle:
                return new Vector3(
                    radius * Mathf.Cos(angle),
                    2f,
                    radius * Mathf.Sin(angle)
                );

            case PatternType.Infinity:
                return new Vector3(
                    radius * Mathf.Cos(angle) * 0.7f,
                    2f,
                    radius * Mathf.Sin(2f * angle) * 0.4f
                );

            case PatternType.Wave:
                return new Vector3(
                    radius * Mathf.Cos(angle),
                    2f + Mathf.Sin(3f * angle) * 0.5f,
                    radius * Mathf.Sin(angle) * 0.5f
                );

            case PatternType.Triangle:
                // Triangular pattern using modulo and conditional logic
                float normalizedAngle = angle % (2f * Mathf.PI);
                float segment = (2f * Mathf.PI) / 3f; // 3 segments for triangle
                float triX = 0f, triZ = 0f;

                if (normalizedAngle < segment)
                {
                    // First segment
                    float t = normalizedAngle / segment;
                    triX = Mathf.Lerp(1f, -0.5f, t);
                    triZ = Mathf.Lerp(0f, 0.866f, t);
                }
                else if (normalizedAngle < 2f * segment)
                {
                    // Second segment
                    float t = (normalizedAngle - segment) / segment;
                    triX = Mathf.Lerp(-0.5f, -0.5f, t);
                    triZ = Mathf.Lerp(0.866f, -0.866f, t);
                }
                else
                {
                    // Third segment
                    float t = (normalizedAngle - 2f * segment) / segment;
                    triX = Mathf.Lerp(-0.5f, 1f, t);
                    triZ = Mathf.Lerp(-0.866f, 0f, t);
                }

                return new Vector3(
                    radius * triX * 0.8f,
                    2f,
                    radius * triZ * 0.8f
                );

            case PatternType.Square:
                // Square pattern using sign function
                float squareX = Mathf.Sign(Mathf.Cos(angle));
                float squareZ = Mathf.Sign(Mathf.Sin(angle));
                return new Vector3(
                    radius * squareX * 0.6f,
                    2f + Mathf.Abs(Mathf.Sin(angle * 4f)) * 0.2f,
                    radius * squareZ * 0.6f
                );

            case PatternType.Figure8:
                return new Vector3(
                    radius * Mathf.Sin(angle) * 0.8f,
                    2f,
                    radius * Mathf.Sin(2f * angle) * 0.4f
                );

            case PatternType.Lissajous:
                // Complex Lissajous curve (2:3 ratio)
                return new Vector3(
                    radius * Mathf.Sin(2f * angle) * 0.7f,
                    2f + Mathf.Sin(angle) * 0.3f,
                    radius * Mathf.Sin(3f * angle) * 0.7f
                );

            case PatternType.Spiral:
                // Spiral with increasing radius
                float spiralProgress = (angle % (4f * Mathf.PI)) / (4f * Mathf.PI);
                float spiralRadius = 0.5f + spiralProgress * 2f;
                return new Vector3(
                    spiralRadius * Mathf.Cos(angle),
                    2f,
                    spiralRadius * Mathf.Sin(angle)
                );

            default:
                return Vector3.zero;
        }
    }

    // Public API for other scripts
    public void AddBall(GameObject ball, PatternType pattern, float speed = 1f, float customRadius = -1f)
    {
        BallPatternData newData = new BallPatternData
        {
            ballObject = ball,
            patternType = pattern,
            movementSpeed = speed,
            phaseOffset = Random.Range(0f, 6.28f),
            radius = customRadius > 0 ? customRadius : baseRadius,
            currentAngle = 0f,
            isActive = true
        };
        allBallsData.Add(newData);
    }

    public PatternType GetBallPattern(GameObject ball)
    {
        BallPatternData data = allBallsData.Find(d => d.ballObject == ball);
        return data != null ? data.patternType : PatternType.Circle;
    }

    public void SetBallSpeed(GameObject ball, float newSpeed)
    {
        BallPatternData data = allBallsData.Find(d => d.ballObject == ball);
        if (data != null) data.movementSpeed = newSpeed;
    }

    public void SetBallPattern(GameObject ball, PatternType newPattern)
    {
        BallPatternData data = allBallsData.Find(d => d.ballObject == ball);
        if (data != null) data.patternType = newPattern;
    }

    public void SetAllBallsActive(bool active)
    {
        foreach (BallPatternData data in allBallsData)
        {
            data.isActive = active;
        }
    }

    // Debug method to see all ball patterns in console
    public void PrintBallPatterns()
    {
        foreach (BallPatternData data in allBallsData)
        {
            if (data.ballObject != null)
            {
                Debug.Log(data.ballObject.name + " - Pattern: " + data.patternType + " - Speed: " + data.movementSpeed);
            }
        }
    }
}