using UnityEngine;

public class SyncLocalizationBridge : MonoBehaviour
{
    public void nextItem()
    {
        SyncLocalization.instance.nextItem();
    }

    public void prevItem()
    {
        SyncLocalization.instance.prevItem();
    }
}
