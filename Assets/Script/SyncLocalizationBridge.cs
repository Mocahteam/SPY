using UnityEngine;

public class SyncLocalizationBridge : MonoBehaviour
{
    public void ChangeLang(LangOption opt)
    {
        SyncLocalization.instance.ChangeLang(opt.lang);
    }
}
