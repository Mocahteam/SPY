
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class Copy : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void TryToCopy(string txt); // call javascript => send txt to html to copy in clipboard
    
    public void copyCode(TMP_Text code)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            TryToCopy(code.text);
    }
}
