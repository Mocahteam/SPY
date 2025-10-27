using UnityEngine;

public class SyncLocalizationBridge : MonoBehaviour
{
    public void changeLang(LangOption opt)
    {
        SyncLocalization.instance.changeLang(opt.lang);
    }
}
