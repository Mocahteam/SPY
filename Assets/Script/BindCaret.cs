using FYFY;
using TMPro;
using UnityEngine;

public class BindCaret : MonoBehaviour
{
    public void bindCaret()
    {
        TMP_SelectionCaret caret = GetComponentInChildren<TMP_SelectionCaret>(true);
        if (!GameObjectManager.isBound(caret.gameObject))
            GameObjectManager.bind(caret.gameObject);
    }
}
