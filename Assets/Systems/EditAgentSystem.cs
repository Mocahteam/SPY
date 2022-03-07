using UnityEngine;
using FYFY;
using TMPro;

public class EditAgentSystem : FSystem {
	// Systeme qui permet de gerer tous ce que l'on peux éditer, faire varier sur l'agent
	// Actuellement le nom

	public static EditAgentSystem instance;

	// On récupére les agents pouvant être édité
	private Family agent_f = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef)));

	// Pour voir si le nom de l'agent change
	private string oldNameAgent = "";

	//GameData object need for param level
	private GameObject gameData;
	// Canvas pour récupe les données des agents modifiées par le joueur
	public GameObject agentCanvas;

	public EditAgentSystem()
	{
		if(Application.isPlaying)
		{
			//Si l'objet gameData et vide on le cherche dans la scéne
			if(gameData == null)
            {
				gameData = GameObject.Find("GameData");
				// Si pas d'objet GameData dans la scéne, on affiche un message d'erreur
				if(gameData == null)
                {
					//Affiche un message d'erreur et une validation permettant de revenir à l'écran titre
				}
			}

			//Si l'objet gameData et vide on le cherche dans la scéne
			if (agentCanvas == null)
			{
				agentCanvas = GameObject.Find("AgentCanvas");
				// Si pas d'objet GameData dans la scéne, on affiche un message d'erreur
				if (agentCanvas == null)
				{
					//Affiche un message d'erreur et une validation permettant de revenir à l'écran titre
				}
			}
		}
		instance = this;
	}

	protected override void onStart()
	{
		// Si la compétence nameObject n'est pas activé on donne un nom aux agents
		int nbAgent = 1;
		foreach (GameObject agent_go in agent_f)
		{
			if (!agent_go.GetComponent<AgentEdit>().editName)
			{
				agent_go.GetComponent<AgentEdit>().agentName = "Agent" + nbAgent;
				nbAgent++;
			}
		}
	}

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		foreach (GameObject agent_go in agent_f)
		{
			Debug.Log("Agent name : " + agent_go.GetComponent<AgentEdit>().agentName);
			// On regarde si le nom des agents peuvent être édité
			if (agent_go.GetComponent<AgentEdit>().editName)
            {
				changeName(agent_go);
            }
		}
	}

	private void changeName(GameObject agent)
    {
        // On regarde si la valeur du nom de l'agent à changé
        // Si oui, on le met à jour la variable agentName
        if (agentCanvas.transform.Find("Container(Clone)").gameObject.activeSelf && agentCanvas.transform.Find("Container(Clone)").Find("Header").Find("agentName").GetComponent<TMP_InputField>().text != agent.GetComponent<AgentEdit>().agentName)
        {
			agent.GetComponent<AgentEdit>().agentName = agentCanvas.transform.Find("Container(Clone)").Find("Header").Find("agentName").GetComponent<TMP_InputField>().text;
		}
	}
}