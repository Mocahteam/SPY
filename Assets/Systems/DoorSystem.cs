using UnityEngine;
using FYFY;
using System.Collections;

public class DoorSystem : FSystem {

	private Family doorGO = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position)), new AnyOfTags("Door"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
	private Family activableConsoleGO = FamilyManager.getFamily(new AllOfComponents(typeof(Activable),typeof(Position),typeof(AudioSource)));
	private Family newCurrentAction_f = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));

	public DoorSystem(){
		newCurrentAction_f.addEntryCallback(onNewCurrentAction);
	}

	private void onNewCurrentAction(GameObject unused){
        //Check Activations
        foreach (GameObject activable in activableConsoleGO)
        {
            if (activable.GetComponent<Activable>().isActivated && !activable.GetComponent<Activable>().isFullyActivated)
            {
                activate(activable);
            }
        }
	}

	private void activate(GameObject go){
		go.GetComponent<Activable>().isFullyActivated = true;
		foreach(int id in go.GetComponent<Activable>().slotID){
			foreach(GameObject slotGo in doorGO){
				if(slotGo.GetComponent<ActivationSlot>().slotID == id){
					switch(slotGo.GetComponent<ActivationSlot>().type){
						case ActivationSlot.ActivationType.Destroy:
							MainLoop.instance.StartCoroutine(doorDestroy(slotGo));
							break;
					}
				}
			}
		}
	}

	private IEnumerator doorDestroy(GameObject go){

		yield return new WaitForSeconds(0.3f);

		go.GetComponent<Renderer>().enabled = false;
		go.GetComponent<AudioSource>().Play();
		
		yield return new WaitForSeconds(0.5f);

		GameObjectManager.setGameObjectState(go, false);
		//GameObjectManager.unbind(go);
		//Object.Destroy(go);
	}
}