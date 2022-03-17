using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// agentSelect
///		Pour enregistrer sur qu'elle agents le systéme va travaillé
/// changeName
///		Pour changer le nom d'un agent
///	changeNameAgentExterneElement
///		
///	linkScriptContainer
///		Création d'un lien entre un agent et un script container du même nom
///	dislinkAgent
///		
///	dislinkScriptContainer
///		Suppression d'un lien entre un agent et un script container du même nom (principalement car l'un des deux partie va changer de nom)
///	changeNameAssociedElement
///		
/// </summary>

public class EditAgentSystem : FSystem 
{
	// Les familles
	private Family agent_f = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On récupére les agents pouvant être édité
	private Family viewportContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(ViewportContainer))); // Les container contenant les container éditable

	// Les variables
	private GameObject agentSelected = null;

	// L'instance
	public static EditAgentSystem instance;

	public EditAgentSystem()
	{
		instance = this;
	}


	// Enregistre l'agent selectionné dans la variable agentSelected
	public void agentSelect(BaseEventData agent)
    {
		Debug.Log("agent selectionné : " + agent.selectedObject.name);
		agentSelected = agent.selectedObject;
	}


	// Associe le nouveau nom reçue à l'agent selectionné
	public void changeName(string newName)
    {
		// On supprime le lien entre l'agent et le container (vue que le nom va changer)
		dislinkScriptContainer(agentSelected);
        // Si le changement de nom entre l'agent et le container est automatique, on change aussi le nom du container
        if (agentSelected.GetComponent<AgentEdit>().editNameAuto)
        {
			changeNameAssociedElement(agentSelected.GetComponent<AgentEdit>().agentName, newName);
		}
		// On met à jours le nom de l'agent
		agentSelected.GetComponent<AgentEdit>().agentName = newName;
		// On relie l'agent au script container du même nom
		linkScriptContainer(agentSelected);
	}

	// Chengement du nom d'un agent par un element extérieur
	public void changeNameAgentExterneElement(string oldName, string newName)
    {
		// On cherche l'agent ayant le même nom
		foreach(GameObject agent in agent_f)
        {
			// On trouve le bon agent
			if(agent.GetComponent<AgentEdit>().agentName == oldName)
            {
				// On change le nom de l'agent
				agent.GetComponent<AgentEdit>().agentName = newName;
				// On change l'affichage du nom dans son container perso
				agent.GetComponent<ScriptRef>().uiContainer.GetComponentInChildren<TMP_InputField>().text = newName;
				// On l'associe au nouveau container
				linkScriptContainer(agent);
			}
        }
    }

	// Si le nom est le même, met en lien avec un container script
	public void linkScriptContainer(GameObject agent)
	{
		// On parcourt la liste des container éditable
		foreach(GameObject container in viewportContainer_f)
        {
			// si un contenaire à le même nom que l'agent, alors on les lie ensemble
			if(container.GetComponentInChildren<UITypeContainer>().associedAgentName == agent.GetComponent<AgentEdit>().agentName)
            {
				container.GetComponentInChildren<UITypeContainer>().agentAssocied = agent;
				agent.GetComponent<ScriptRef>().scriptContainer = container.transform.Find("ScriptContainer").gameObject;
			}
        }
	}


	// Cherche l'agent dont on a reçue le nom pour apeller la fonction dislinkContainer (false) ou linkContainer (true)
	public void dislinkOrLinkAgent(string agentName, bool value)
    {
		foreach(GameObject agent in agent_f)
        {
			if(agent.GetComponent<AgentEdit>().agentName == agentName)
            {
                if (value)
                {
					linkScriptContainer(agent);

				}
                else
                {
					dislinkScriptContainer(agent);
				}
			}
        }
    }

	// Si le nom est le même, supprime le lien avec un container script
	public void dislinkScriptContainer(GameObject agent)
	{
		// On parcourt la liste des container éditable
		foreach (GameObject container in viewportContainer_f)
		{
			// si un contenaire à le même nom que l'agent, alors on les dissocie
			if (container.GetComponentInChildren<UITypeContainer>().associedAgentName == agent.GetComponent<AgentEdit>().agentName)
			{
				container.GetComponentInChildren<UITypeContainer>().agentAssocied = null;
				agent.GetComponent<ScriptRef>().scriptContainer = null;
			}
		}
	}

	// Change le nom auquel l'agent est associer
	public void changeNameAssociedElement(string oldName, string newName)
    {
		UISystem.instance.changeNameContainerExterneElement(oldName, newName);
	}
}