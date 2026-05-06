using System;
using UnityEngine;

public class TransformableBlock : MonoBehaviour
{
    public bool isTransformed = false;
    public bool isGuard = false;

    private Sprite originalSprite;
    private string originalTag;
    private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        originalSprite = gameObject.GetComponent<Sprite>();
        originalTag = gameObject.tag;
    }

    public void TransformBlock(Sprite newSprite, string newTag)
    {
        isTransformed = true;
        spriteRenderer.sprite = newSprite;
        gameObject.tag = newTag;
    }

    public void RevertToOriginal()
    {
        isTransformed = false;
        spriteRenderer.sprite = originalSprite;
        gameObject.tag = originalTag;
    }
}
