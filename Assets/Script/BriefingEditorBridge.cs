using UnityEngine;

public class BriefingEditorBridge : MonoBehaviour
{
    public void prepareBriefingsEditor(GameObject src)
    {
        BriefingEditor.instance.prepareBriefingsEditor(gameObject.GetComponent<DataLevelBehaviour>(), src);
    }

    public void saveBriefings()
    {
        BriefingEditor.instance.saveBriefings();
    }

    public void addNewBriefing(GameObject parent)
    {
        BriefingEditor.instance.addNewBriefing(parent);
    }

    public void removeItemFromParent()
    {
        BriefingEditor.instance.removeItemFromParent(gameObject);
    }

    public void moveItemInParent(int step)
    {
        BriefingEditor.instance.moveItemInParent(gameObject, step);
    }

    public void markLayoutForRebuild(RectTransform transform)
    {
        BriefingEditor.instance.markLayoutForRebuild(transform);
    }
}
