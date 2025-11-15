using UnityEngine;

public class game_mgr_2 : MonoBehaviour
// GameManager.cs

{
    [Header("References")]
    public pattern_move_mgr movementManager;
    public GameObject ballPrefab; // prefab must have ball_script + SpriteRenderer
    public int ballCount = 3;

    [Header("Spawn")]
    public float spawnRadius = 0.5f;
    public Color[] defaultColors;

    void Start()
    {
/*        if (movementManager == null) movementManager = FindObjectOfType<pattern_move_mgr>();
        if (ballPrefab == null) { Debug.LogError("Ball prefab missing in GameManager"); return; }

        // create N balls and assign patterns and colors
        for (int i = 0; i < ballCount; i++)
        {
            GameObject go = Instantiate(ballPrefab, movementManager.center + Random.insideUnitCircle * spawnRadius, Quaternion.identity);
            ball_script bs = go.GetComponent<ball_script>();
            if (bs == null) bs = go.AddComponent<ball_script>();

            bs.movementManager = movementManager;

            // assign color
            if (defaultColors != null && defaultColors.Length > 0)
            {
                Color c = defaultColors[i % defaultColors.Length];
                bs.SetColor(c);
            }

            // assign pattern: simple round-robin from library
            if (movementManager.patternLibrary != null && movementManager.patternLibrary.Length > 0)
            {
                int index = i % movementManager.patternLibrary.Length;
                // phase offset to separate orbits
                float phaseOffset = (i * Mathf.PI * 2f / Mathf.Max(1, ballCount));
                movementManager.AssignPatternToBall(bs, index, phaseOffset);
            }
        }*/
    }
}
