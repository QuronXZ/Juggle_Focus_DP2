using UnityEngine;

public class ball_script : MonoBehaviour
{
    //public SpriteRenderer spriteRenderer;
    public Color ballColor = Color.red;

    void Start()
    {
        //if (spriteRenderer == null)
        //    spriteRenderer = GetComponent<SpriteRenderer>();

        //spriteRenderer.color = ballColor;
    }

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
    }
}

