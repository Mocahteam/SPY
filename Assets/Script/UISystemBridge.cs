using UnityEngine;

// Used in ForBloc
public class UISystemBridge : MonoBehaviour
{
	public void onlyPositiveInteger(string newValue)
    {
        UISystem.instance.onlyPositiveInteger(gameObject, newValue);
    }
}
