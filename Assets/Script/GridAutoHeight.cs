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
        // Calcul du pire nombre de colonne
        int cols = Mathf.Max(1, Mathf.FloorToInt((((RectTransform)grid.transform).rect.width - grid.padding.left - grid.padding.right) / (grid.cellSize.x + grid.spacing.x )));
        // On corrige le pire nombre de colonne pour être au plus juste, en effet avec le calcul précédent on a compté un spacing.x pour chaque cell hors en réalité on en a un de moins que le nombre de cellule( pour 3 colonnes on n'a que 2 espacements). Donc on teste si avec une colonne de plus et un espacement de moins ça ne passerait pas quand même
        if (cols > 1 && grid.padding.left + grid.padding.right + (cols+1) * (grid.cellSize.x + grid.spacing.x) - grid.spacing.x <= ((RectTransform)grid.transform).rect.width)
            cols++;
        int rows = Mathf.CeilToInt((float)count / cols);
        float height =
            rows * grid.cellSize.y +
            Mathf.Max(0, rows - 1) * grid.spacing.y +
            grid.padding.top +
            grid.padding.bottom;

        if (layout.preferredHeight != height)
        {
            layout.preferredHeight = height;
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }
    }
}

