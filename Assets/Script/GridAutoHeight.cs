using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GridAutoHeight : MonoBehaviour
{
    public GridLayoutGroup grid;
    public LayoutElement layout;

    
    void OnEnable()
    {
        Recalculate();
    }

    void OnTransformChildrenChanged()
    {
        Recalculate();
    }

    private void OnRenderObject()
    {
        Recalculate();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        Recalculate();
    }
#endif

    public void Recalculate()
    {
        if (grid == null || layout == null) return;

        int count = 0;
        foreach (Transform child in grid.transform)
            count += child.gameObject.activeInHierarchy ? 1 : 0;
        int cols = Mathf.Max(1, Mathf.FloorToInt((((RectTransform)grid.transform).rect.width - grid.padding.left - grid.padding.right) / (grid.cellSize.x + grid.spacing.x )));
        int rows = Mathf.CeilToInt((float)count / cols);

        float height =
            rows * grid.cellSize.y +
            Mathf.Max(0, rows - 1) * grid.spacing.y +
            grid.padding.top +
            grid.padding.bottom;

        if (layout.preferredHeight != height)
        {
            layout.preferredHeight = height;
            LayoutRebuilder.MarkLayoutForRebuild(
                (RectTransform)transform
            );
        }
    }
}

