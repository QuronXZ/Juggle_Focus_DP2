using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "JuggleFocus/PatternDefinition", fileName = "Pattern_")]
public class PatternDefinition : ScriptableObject
{
    //public string patternName = "NewPattern";
    public string patternName;
    public List<Vector2> points = new List<Vector2>(); // pattern waypoints
    public float scale = 2f;
    public enum PatternType { Circle, Lissajous, Infinity, Triangle, Line, Mshape, Sshape, CustomSpline }
    public PatternType patternType = PatternType.Lissajous;

    [Header("Base / Orbit")]
    public float orbitRadius = 1.2f;     // base circular orbit radius
    public float orbitSpeed = 1.0f;      // ω - angular speed of base orbit
    public float orbitPhase = 0f;        // φ phase offset

    [Header("Pattern Offset (Lissajous-style)")]
    public float A = 0.5f;               // X amplitude
    public float B = 0.5f;               // Y amplitude
    public float a = 2.0f;               // X frequency
    public float b = 1.0f;               // Y frequency
    public float deltaX = 0f;            // phase shift for X
    public float deltaY = 0f;            // phase shift for Y

    [Header("Generic")]
    public float globalScale = 1.0f;     // scale the whole pattern
    public float speedMultiplier = 1.0f; // global multiplier for time
    [Range(0f, 1f)]
    public float bounceDamp = 0.85f;     // damping on "bounce"

    [Header("Optional: spline points (for CustomSpline)")]
    public Vector2[] splinePoints;
}
