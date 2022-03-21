using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 
/// agentSelect
///		Pour enregistrer sur qu'elle agents le systéme va travaillé
///	modificationAgent
///		Pour les appel extérieurs, permet de trouver l'agent (et le considéré comme selectionné) en fonction de son nom
///		Renvoie True si trouvé, sinon false
/// setAgentName
///		Pour changer le nom d'un agent
///	majDisplayCardAgent
///		Met à jour l'affichage des info de l'agent dans ça fiche
///	newScriptContainerLink
///		Supprime le lien existant avec le container Script actuelle et recherche le containe script identique au nouveau nom de l'agent
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
		agentSelected = agent.selectedObject;
	}


	// Utilisé principalement par les systéme extérieur
	// Définie l'agent sur lequel les modifications seront opporté
	// Renvoie True si agent trouvé, sinon false
	public bool modificationAgent(string nameAgent)
    {
		foreach (GameObject agent in agent_f)
        {
			//Debug.Log("Nom agent : " + agent.GetComponent<AgentEdit>().agentName);
			if (agent.GetComponent<AgentEdit>().agentName == nameAgent)
            {
				agentSelected = agent;
				return true;
			}
        }
		return false;
	}


	// Associe le nouveau nom reçue à l'agent selectionné
	// Met à jours son affichage dans ça fiche
	// Met à jours le lien qu'il à avec le script container du même nom
	public void setAgentName(string newName)
    {
        if (agentSelected.GetComponent<AgentEdit>().editName)
        {
			// Si le changement de nom entre l'agent et le container est automatique, on change aussi le nom du container
			if (agentSelected.GetComponent<AgentEdit>().editNameAuto)
			{
				UISystem.instance.setContainerName(agentSelected.GetComponent<AgentEdit>().agentName, newName);
			}
			// On met à jours le nom de l'agent
			agentSelected.GetComponent<AgentEdit>().agentName = newName;
			// On met à jour l'affichage du nom dans le containe fiche de l'agent
			majDisplayCardAgent(newName);
			// On associe la fiche du l'agent au nouveau container
			newScriptContainerLink();
		}
	}

	// Met à jours l'affichage du nom de l'agent dans ça fiche
	public void majDisplayCardAgent(string newName)
    {
		agentSelected.GetComponent<ScriptRef>().uiContainer.GetComponentInChildren<TMP_InputField>().text = newName;
		UISystem.instance.refreshUI();
	}

	// Associe la fiche de l'agent au nouveau container (correspondant au nom de l'agent)
	public void newScriptContainerLink()
    {
		// On supprime le lien actuel
		agentSelected.GetComponent<ScriptRef>().scriptContainer = null;
		// On parcourt la liste des container pour y associer celui du même nom (si il existe)
		foreach (GameObject container in viewportContainer_f)
		{
			if (container.GetComponentInChildren<UITypeContainer>().associedAgentName == agentSelected.GetComponent<AgentEdit>().agentName)
			{
				agentSelected.GetComponent<ScriptRef>().scriptContainer = container.transform.Find("ScriptContainer").gameObject;
			}
		}
	}

}