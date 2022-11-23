 using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LevelEditorBrushPreviewController : MonoBehaviour
{
    private static LevelEditorBrushPreviewController Singleton;

    public static void SetPosition(Vector3 center)
    {
        if (Singleton == null) return;

        Singleton.transform.position = center;
    }

    public static void SetSize(float size)
    {
        if (Singleton == null) return;

        Singleton.transform.localScale = new Vector3(size, size, 1);
    }

    public static void SetStrength(float hardness)
    {
        if (Singleton == null) return;

        Singleton.lineRenderer.startColor = new Color(1f, 1f, 1f, hardness);
        Singleton.lineRenderer.endColor = Singleton.lineRenderer.startColor;
    }

    public static void SetVisible(bool visible)
    {
        if (Singleton == null) return;

        Singleton.lineRenderer.enabled = visible;
    }

    private LineRenderer lineRenderer;

    private void Awake()
    {
        Singleton = this;

        lineRenderer = GetComponent<LineRenderer>();

        var positions = new List<Vector3>();

        for(float i = 0; i < 1f; i += 0.02f)
        {
            positions.Add(new Vector3(Mathf.Sin(i * 360 * Mathf.Deg2Rad) / 2, Mathf.Cos(i * 360 * Mathf.Deg2Rad) / 2, 0));
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }
}
