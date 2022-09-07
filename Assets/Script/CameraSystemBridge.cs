using UnityEngine;

public class CameraSystemBridge : MonoBehaviour
{
    // Active ou desactive le systéme
    public void PauseCameraSystem(bool value)
    {
        // Comment faire pour mettre le systéme en pause puis le relancer
        CameraSystem.instance.Pause = value;
    }

    public void locateAgent(LinkedWith agent)
    {
        CameraSystem.instance.focusOnAgent(agent.target);
    }
}
