using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class FadeSpriteRenderer : MonoBehaviour
{
    [SerializeField]
    [Range(0,1)]
    private float min;
    [SerializeField]
    [Range(0,1)]
    private float max = 1;
    [SerializeField]
    private float speed = 0.5f;

    private float sin;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        sin = (Mathf.Sin(Time.time * speed) + 1) / 2;

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(min, max, sin));
    }
}
