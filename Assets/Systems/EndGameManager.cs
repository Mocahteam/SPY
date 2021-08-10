using UnityEngine;
using FYFY;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// This system check if the end of the level is reached
/// </summary>
public class EndGameManager : FSystem {
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
    private Family newCurrentAction_f = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));
	private Family exitGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(AudioSource)), new AnyOfTags("Exit"));
    private Family endpanel_f = FamilyManager.getFamily(new AllOfComponents(typeof(Image), typeof(AudioSource)), new AnyOfTags("endpanel"));
	private GameObject endPanel;

	public EndGameManager(){
		if (Application.isPlaying)
		{
			newCurrentAction_f.addExitCallback(onCurrentActionRemoved);
			endPanel = endpanel_f.First();
		}
    }

	// each time a current action is removed, we check if the level is over
	private void onCurrentActionRemoved(int unused){
		MainLoop.instance.StartCoroutine(delayCheckEnd());
	}
	
	private bool playerHasCurrentAction(){
		foreach(GameObject go in newCurrentAction_f){
			if(go.GetComponent<CurrentAction>().agent.CompareTag("Player"))
				return true;
		}
		return false;
	}

	private IEnumerator delayCheckEnd(){
		// wait 2 frames before checking if a new currentAction was produced
		yield return null; // this frame the currentAction is removed
		yield return null; // this frame a probably new current action is created
		// Now, families are informed if new current action was produced, we can check if no currentAction exists on players and if all players are on the end of the level
		if(!playerHasCurrentAction()){
			int nbEnd = 0;
			// parse all players
			foreach (GameObject player in playerGO)
			{
				// parse all ends
				foreach (GameObject exit in exitGO)
				{
					// check if positions are equals
					if (player.GetComponent<Position>().x == exit.GetComponent<Position>().x && player.GetComponent<Position>().z == exit.GetComponent<Position>().z)
					{
						nbEnd++;
						// if all players reached end position
						if (nbEnd >= playerGO.Count)
							// trigger end
							GameObjectManager.addComponent<NewEnd>(endPanel, new { endType = NewEnd.Win });
					}
				}				
			}				
		}
	}
}
