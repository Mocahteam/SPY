using UnityEngine;

public class DoorAndConsoleManagerBridge : MonoBehaviour
{
	public void startNextPathAnimation()
    {
        DoorAndConsoleManager.instance.startNextPathAnimation(this.gameObject);
    }
}
