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

    void Recalculate()
    {
        if (grid == null || layout == null) return;

        int count = grid.transform.childCount;
        int cols = Mathf.Max(1, Mathf.FloorToInt(((RectTransform)grid.transform).rect.width / (grid.cellSize.x + grid.spacing.x)));
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

