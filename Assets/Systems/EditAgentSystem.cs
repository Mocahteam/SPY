using UnityEngine;
using FYFY;
using TMPro;

public class EditAgentSystem : FSystem {
	// Systeme qui permet de gerer tous ce que l'on peux éditer, faire varier sur l'agent
	// Actuellement le nom

	public static EditAgentSystem instance;

	// On récupére les agents pouvant être édité
	private Family agent_f = FamilyManager.getFamily(new AllOfComponents(typeof(AgentEdit), typeof(ScriptRef)));

	public EditAgentSystem()
	{
		if(Application.isPlaying)
		{

		}
		instance = this;
	}

	protected override void onStart()
	{

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

	}
}