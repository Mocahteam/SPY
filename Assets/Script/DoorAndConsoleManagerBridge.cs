using UnityEngine;

public class DoorAndConsoleManagerBridge : MonoBehaviour
{
	public void StartNextPathAnimation()
    {
        DoorAndConsoleManager.instance.StartNextPathAnimation(this.gameObject);
    }
}
