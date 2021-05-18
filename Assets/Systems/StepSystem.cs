using UnityEngine;
using FYFY;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Collections;

public class StepSystem : FSystem {

    private Family newEnd_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
    private Family firstStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(FirstStep)));
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    //private Family highlightedItems = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType), typeof(CurrentAction)));
    //private Family visibleContainers = FamilyManager.getFamily(new AllOfComponents(typeof(CanvasRenderer), typeof(ScrollRect), typeof(AudioSource)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); 
	//private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
    private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));
    private float timeStepCpt;
	private static float timeStep = 1.5f;
	private GameData gameData;
    //private bool autoExecution;
	public StepSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		gameData.nbStep = 0;
		timeStepCpt = timeStep;
        newStep_f.addEntryCallback(onNewStep);
        firstStep_f.addEntryCallback(onFirstStep);
        //autoExecution = true;
    }
    
    private void onNewStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<NewStep>());
        /*
        await Task.Delay((int)timeStep*1000);
        Debug.Log("test");
        highlight();
        */        
        timeStepCpt = timeStep;
        gameData.nbStep--;
        Debug.Log("StepSystem End");
        //stepFinished = true;
        /*
        if(gameData.nbStep == 0){
            Debug.Log("end");
            await Task.Delay((int)timeStep*1000);
            Debug.Log("step+1");
            foreach(GameObject highlightedGO in highlightedItems){
                if (highlightedGO.GetComponent<CurrentAction>() != null){
                    Debug.Log("remove");
                    GameObjectManager.removeComponent<CurrentAction>(highlightedGO);
                }
            }
        }*/
    }
    private void onFirstStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<FirstStep>());
        timeStepCpt = timeStep;
        gameData.nbStep--;
        Debug.Log("FirstStep End");

    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {

		//Organize each steps
		if(currentActions.Count > 0 && newEnd_f.Count == 0){
            gameData.totalExecute++;
            //activate step
            if (timeStepCpt <= 0 && playerHasNextAction())
            {
                GameObjectManager.addComponent<NewStep>(MainLoop.instance.gameObject);
                gameData.totalStep++;
            }
            else
                timeStepCpt -= Time.deltaTime;

		}
	}

    private bool playerHasNextAction(){
		CurrentAction act;
		foreach(GameObject go in currentActions){
			act = go.GetComponent<CurrentAction>();
			if(act.agent != null && act.agent.CompareTag("Player") && act.GetComponent<BaseElement>().next != null){
				return true;                
            }
		}
        //execution finished
		if(MainLoop.instance.gameObject.GetComponent<PlayerIsMoving>()){
			Debug.Log("fin exec");
            Pause = true;
			GameObjectManager.removeComponent<PlayerIsMoving>(MainLoop.instance.gameObject);     
		}
        return false;
    }

    public void autoExecuteStep(bool on){
        Pause = !on;
        /*
        if(on){
           autoExecution = true; 
        }
        */
        //autoExecution = on;
    }


    private async void delayPause(){
        await Task.Delay((int)timeStep*1000);
        Pause = true;
        /*
        if(!stepFinished){
            yield return null;
        }
        else{
            stepFinished = true;
            Pause = true;
        }*/

    }


    public void goToNextStep(){
        Pause = false;
        //autoExecution = false;
        if(timeStepCpt <= 0 && playerHasNextAction()){
            GameObjectManager.addComponent<NewStep>(MainLoop.instance.gameObject);
            gameData.totalStep++;   
        }
        delayPause();
       //MainLoop.instance.StartCoroutine(delayPause());
    }

}