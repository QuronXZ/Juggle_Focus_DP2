using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Ball Identity")]
    public string ballColor;
    public string ballPattern;
    //public string ballID; // "Red_Dots", "Blue_Stripes", etc.

    [Header("Movement")]
    public float fallSpeed = 5f;

    void Update()
    {
        // Move downward
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Destroy when off screen
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    // Check if this ball matches target
    public bool MatchesTarget(string targetColor, string targetPattern)
    {
        return ballColor == targetColor && ballPattern == targetPattern;
    }

    // Quick ID check
/*    public bool MatchesTargetID(string targetID)
    {
        //return ballID == targetID;
    }*/
}