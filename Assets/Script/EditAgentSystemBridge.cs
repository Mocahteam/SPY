using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditAgentSystemBridge : MonoBehaviour
{
	public void agentSelect(BaseEventData agent)
    {
		Debug.Log("agent selectionné");
		EditAgentSystem.instance.agentSelect(agent);
	}

	// Methode appellée pour changer le nom de l'agent
	public void setAgentName(string newName)
	{
		EditAgentSystem.instance.setAgentName(newName);
	}
}
