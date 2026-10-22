using UnityEngine;

public class BriefingEditorBridge : MonoBehaviour
{
    public void PrepareBriefingsEditor(GameObject src)
    {
        BriefingEditor.instance.PrepareBriefingsEditor(gameObject.GetComponent<DataLevelBehaviour>(), src);
    }

    public void SaveBriefings()
    {
        BriefingEditor.instance.SaveBriefings();
    }

    public void AddNewBriefing(GameObject parent)
    {
        BriefingEditor.instance.AddNewBriefing(parent);
    }

    public void RemoveItemFromParent()
    {
        BriefingEditor.instance.RemoveItemFromParent(gameObject);
    }

    public void MoveItemInParent(int step)
    {
        BriefingEditor.instance.MoveItemInParent(gameObject, step);
    }

    public void MarkLayoutForRebuild(RectTransform transform)
    {
        BriefingEditor.instance.MarkLayoutForRebuild(transform);
    }
}
