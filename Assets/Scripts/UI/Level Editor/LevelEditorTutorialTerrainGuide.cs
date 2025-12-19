using UnityEngine;

public class LevelEditorTutorialTerrainGuide : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private float lastChecked;
    private bool isValid = false;

    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if(Time.time - lastChecked > 0.25f)
        {
            lastChecked = Time.time;

            isValid = !LevelEditorManager.GetLevelData().IsPointInLevelNew(transform.position + new Vector3(18, 18));
        }

        if (!isValid)
        {
            spriteRenderer.color = new Color(Mathf.Lerp(0.8f, 1f, Mathf.Sin(Time.time * 2.5f) / 2 + 0.5f), 0, 0, 0.125f);
        }
        else
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, new Color(0, 1, 0, 0.065f), 3f * Time.deltaTime);
        }
    }
}
