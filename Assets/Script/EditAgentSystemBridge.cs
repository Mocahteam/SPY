using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditAgentSystemBridge : MonoBehaviour
{
	// Agent associé au script
	public GameObject agent;

	void Start()
	{

	}

	// Methode appellée pour changer le nom de l'agent
	public void changeName(string newName)
	{	
		agent.GetComponent<AgentEdit>().agentName = newName;
	}

}
