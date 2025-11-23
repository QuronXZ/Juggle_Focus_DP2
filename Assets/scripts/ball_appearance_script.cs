using UnityEngine;

public class ball_appearance_script : MonoBehaviour
{
    [Header("Appearance Settings")]
    public Color ballColor = Color.white;
    public float glowIntensity = 1f;
    public string ballName = "Ball";

    [Header("Ball Properties")]
    public float baseSpeed = 1f;
    public bool isTarget = false;

    private Renderer ballRenderer;
    private Material ballMaterial;
    private Pattern_movement_script patternSystem;
    private Color originalColor;

    void Start()
    {
        InitializeComponents();
        CreateUniqueMaterial();
        UpdateAppearance();

        // Register with pattern system if not already done
        patternSystem = FindFirstObjectByType<Pattern_movement_script>();
    }

    void InitializeComponents()
    {
        ballRenderer = GetComponent<Renderer>();
        if (ballRenderer == null)
        {
            Debug.LogError("ball_appearance_script: No Renderer found on " + gameObject.name);
        }
    }

    void CreateUniqueMaterial()
    {
        // Create a unique material instance for this ball
        ballMaterial = new Material(Shader.Find("Standard"));
        ballRenderer.material = ballMaterial;
        originalColor = ballColor;
    }

    public void UpdateAppearance()
    {
        if (ballMaterial != null)
        {
            ballMaterial.color = ballColor;
            // Enable emission for glow effect
            ballMaterial.EnableKeyword("_EMISSION");
            ballMaterial.SetColor("_EmissionColor", ballColor * glowIntensity);

            // Make material more vibrant
            ballMaterial.SetFloat("_Metallic", 0.7f);
            ballMaterial.SetFloat("_Glossiness", 0.8f);
        }
    }

    public void SetHighlight(bool isHighlighted)
    {
        isTarget = isHighlighted;

        if (isHighlighted)
        {
            // Bright highlight effect
            ballColor = Color.white;
            glowIntensity = 2f;
            // Add a subtle scale increase for emphasis
            transform.localScale = Vector3.one * 1.2f;
        }
        else
        {
            // Revert to normal appearance
            ballColor = originalColor;
            glowIntensity = 1f;
            transform.localScale = Vector3.one;
        }

        UpdateAppearance();
    }

    public void ChangeColor(Color newColor)
    {
        originalColor = newColor;
        if (!isTarget) // Only change if not currently highlighted
        {
            ballColor = newColor;
            UpdateAppearance();
        }
    }

    public void SetSpeed(float newSpeed)
    {
        baseSpeed = newSpeed;
        if (patternSystem != null)
        {
            patternSystem.SetBallSpeed(this.gameObject, newSpeed);
        }
    }

    // Called when pattern system is ready to register this ball
    public void RegisterWithPatternSystem(Pattern_movement_script.PatternType pattern)
    {
        if (patternSystem != null)
        {
            patternSystem.AddBall(this.gameObject, pattern, baseSpeed);
        }
    }

    void OnDestroy()
    {
        // Clean up the material instance
        if (Application.isPlaying)
        {
            Destroy(ballMaterial);
        }
        else
        {
            DestroyImmediate(ballMaterial);
        }
    }
}