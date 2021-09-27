using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GridSizer : MonoBehaviour {
    
    public int rowCount = 2;
    public float heightMul = 1;

    public float padding = -10;

    private RectTransform rect;
    private GridLayoutGroup grid;

    private void Awake() {
        rect = GetComponent<RectTransform>();
        grid = GetComponent<GridLayoutGroup>();
    }

    void Update () {
        float size = rect.rect.width / rowCount + padding;// (grid.padding.left + grid.padding.right) * 2 + grid.spacing.x;

        grid.cellSize = new Vector2(size, size * heightMul);
	}
}
