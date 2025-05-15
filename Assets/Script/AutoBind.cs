using FYFY;
using UnityEngine;

public class AutoBind : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!GameObjectManager.isBound(this.gameObject))
            GameObjectManager.bind(this.gameObject);
    }
}
