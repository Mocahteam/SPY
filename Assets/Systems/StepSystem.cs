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
	private Family scriptIsRunning = FamilyManager.getFamily(new AllOfComponents(typeof(PlayerIsMoving)));
    private float timeStepCpt;
	private static float timeStep = 1.5f;
	private GameData gameData;
    private int nbStep;
    private bool newStepAskedByPlayer;

	public StepSystem(){
        nbStep = 0;
        newStepAskedByPlayer = false;
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		timeStepCpt = timeStep;
        newStep_f.addEntryCallback(onNewStep);
        firstStep_f.addEntryCallback(onFirstStep);
        //reset nbstep on execution end
        scriptIsRunning.addExitCallback(delegate{nbStep = 0;});
    }
    
    private void onNewStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<NewStep>());  
        timeStepCpt = timeStep;
        Debug.Log("StepSystem End");
    }
    private void onFirstStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<FirstStep>());
        timeStepCpt = timeStep;
        gameData.totalStep++;
        nbStep++;
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
                nbStep++;
                if(newStepAskedByPlayer){
                    newStepAskedByPlayer = false;
                    Pause = true;
                }
            }
            else
                timeStepCpt -= Time.deltaTime;

		}
        else{
            MainLoop.instance.StartCoroutine(delayCheckEnd());
        }
	}

    private IEnumerator delayCheckEnd(){
        yield return null;
        yield return null;
        if(currentActions.Count == 0)
            stopExecution();
    }

    private void stopExecution(){
        if(MainLoop.instance.gameObject.GetComponent<PlayerIsMoving>()){
            Debug.Log("fin exec");
            //quick fix for several PlayerIsMoving
            foreach(PlayerIsMoving p in MainLoop.instance.GetComponents<PlayerIsMoving>()){
                GameObjectManager.removeComponent<PlayerIsMoving>(MainLoop.instance.gameObject);
            }
            Pause = true;
        }
    }

    private bool playerHasNextAction(){
        Debug.Log("playerHasNextAction");
		CurrentAction act;
		foreach(GameObject go in currentActions){
			act = go.GetComponent<CurrentAction>();
			if(act.agent != null && act.agent.CompareTag("Player") && act.GetComponent<BaseElement>().next != null){
				return true;                
            }
		}
        stopExecution();
        return false;
    }

    public void autoExecuteStep(bool on){
        Pause = !on;
    }

    /*
    private async void delayPause(){
        await Task.Delay((int)timeStep*1000);
        Pause = true;
    }
    */

    public void goToNextStep(){
        Pause = false;
        newStepAskedByPlayer = true;
        /*
        if(timeStepCpt <= 0 && playerHasNextAction()){
            GameObjectManager.addComponent<NewStep>(MainLoop.instance.gameObject);
            gameData.totalStep++;   
        }
        */
        //delayPause();
       //MainLoop.instance.StartCoroutine(delayPause());
    }

    public void updateTotalStep(){ //on click on stop button
        gameData.totalStep -= nbStep;
        nbStep = 0;
    }

}