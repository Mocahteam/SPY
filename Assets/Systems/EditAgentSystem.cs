using UnityEngine;
using FYFY;
using TMPro;
using UnityEngine.EventSystems;

/// Ce systéme gére tous les éléments d'éditions des agents par l'utilisateur.
/// Il gére entre autre:
///		Le changement de nom du robot
///		Le changement automatique (si activé) du nom du container associé (si container associé)
///		Le changement automatique (si activé) du nom du robot lorsque l'on change le nom dans le container associé (si container associé)
/// 
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
///		
/// </summary>

public class EditAgentSystem : FSystem 
{
	// Les familles
	private Family agent_f = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef))); // On récupére les agents pouvant être édité

	// Les variables
	public GameObject agentSelected = null;

	// L'instance
	public static EditAgentSystem instance;

	public EditAgentSystem()
	{
		instance = this;
	}

	// Utilisé principalement par les systémes extérieurs
	// Définie l'agent sur lequel les modifications seront opporté
	// Renvoie le composant AgentEdit de l'agent sélectionné s'il a été trouvé, sinon null
	public AgentEdit selectLinkedAgentByName(string nameAgent)
    {
		foreach (GameObject agent in agent_f)
        {
			AgentEdit ae = agent.GetComponent<AgentEdit>();
			if (ae.agentName == nameAgent && ae.editState == AgentEdit.EditMode.Synch)
            {
				agentSelected = agent;
				return agentSelected.GetComponent<AgentEdit>();
			}
        }
		return null;
	}


	// Associe le nouveau nom reçue à l'agent selectionné
	// Met à jours son affichage dans ça fiche
	// Met à jours le lien qu'il a avec le script container du même nom
	public void setAgentName(string newName)
    {
		string oldName = agentSelected.GetComponent<AgentEdit>().agentName;

		if (agentSelected.GetComponent<AgentEdit>().editState != AgentEdit.EditMode.Locked && newName != oldName)
        {
			// On annule la saisie si l'agent est locked ou s'il est synchro et que le nouveau nom choisi est un nom de container editable déjà défini. En effet changer le nom du robot implique de changer aussi le nom du container mais attention il ne peut y avoir de doublons dans les noms des containers editables donc il faut s'assurer que le renommage du container editable a été accepté pour pouvoir valider le nouveau nom de l'agent.
			if (agentSelected.GetComponent<AgentEdit>().editState == AgentEdit.EditMode.Locked || (agentSelected.GetComponent<AgentEdit>().editState == AgentEdit.EditMode.Synch && UISystem.instance.nameContainerUsed(newName)))
            { // on annule la saisie
				agentSelected.GetComponent<ScriptRef>().uiContainer.GetComponentInChildren<TMP_InputField>().text = agentSelected.GetComponent<AgentEdit>().agentName;
			}
			else
			{
				if (agentSelected.GetComponent<AgentEdit>().editState == AgentEdit.EditMode.Synch)
				{
					// On met à jours le nom de tous les agents qui auraient le même nom pour garder l'association avec le container editable
					foreach (GameObject agent in agent_f)
						if (agent.GetComponent<AgentEdit>().agentName == oldName)
						{
							agent.GetComponent<AgentEdit>().agentName = newName;
							agent.GetComponent<ScriptRef>().uiContainer.GetComponentInChildren<TMP_InputField>().text = newName;
						}
					// Puis on demande la mise à jour du nom du container éditable
					UISystem.instance.setContainerName(oldName, newName);
				}
                else
				{
					// on ne modifie que l'agent selectionné
					agentSelected.GetComponent<AgentEdit>().agentName = newName;
				}
				agentSelected.GetComponent<ScriptRef>().uiContainer.GetComponentInChildren<TMP_InputField>().text = newName;
			}

			// On vérifie si on a une association avec les container éditables
			UISystem.instance.refreshUINameContainer();
		}
	}
}