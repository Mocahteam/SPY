using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditAgentSystemBridge : MonoBehaviour
{
	public GameObject agent; // the in game agent associated to the UIContainer

    // Methode appellée pour changer le nom de l'agent
    public void setAgentName(string newName)
	{
		EditAgentSystem.instance.setAgentName(newName);
	}

	// Enregistre l'agent selectionné dans la variable agentSelected
	public void selectAgent()
	{
		EditAgentSystem.instance.agentSelected = agent;
	}
}
