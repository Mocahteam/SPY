using UnityEngine;
using FYFY;
using System.Collections;

/// <summary>
/// Manage steps (automatic simulation or controled by player)
/// </summary>
public class StepSystem : FSystem {

    private Family newEnd_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
    private Family firstStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(FirstStep)));
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));
	private Family scriptIsRunning = FamilyManager.getFamily(new AllOfComponents(typeof(PlayerIsMoving)));
    private float timeStepCpt;
    private static float defaultTimeStep = 1.5f; 
	private static float timeStep = defaultTimeStep;
	private GameData gameData;
    private int nbStep;
    private bool newStepAskedByPlayer;

	public StepSystem()
    {
        if (Application.isPlaying)
        {
            nbStep = 0;
            newStepAskedByPlayer = false;
            GameObject go = GameObject.Find("GameData");
            if (go != null)
                gameData = go.GetComponent<GameData>();
            timeStepCpt = timeStep;
            newStep_f.addEntryCallback(onNewStep);
            firstStep_f.addEntryCallback(onFirstStep);
            //reset nbstep on execution end
            scriptIsRunning.addExitCallback(delegate { nbStep = 0; });
        }
    }
    
    private void onNewStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<NewStep>());  
        timeStepCpt = timeStep;
    }
    private void onFirstStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<FirstStep>());
        timeStepCpt = timeStep;
        gameData.totalStep++;
        nbStep++;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {

		//Organize each steps
		if(currentActions.Count > 0 && newEnd_f.Count == 0){
            
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
            //quick fix for several PlayerIsMoving
            foreach(PlayerIsMoving p in MainLoop.instance.GetComponents<PlayerIsMoving>())
                GameObjectManager.removeComponent<PlayerIsMoving>(MainLoop.instance.gameObject);
            Pause = true;
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
        stopExecution();
        return false;
    }

    // See PauseButton, ExecuteButton, ContinueButton, StopButton and ReloadState buttons in editor
    public void autoExecuteStep(bool on){
        Pause = !on;
    }

    // See NextStepButton in editor
    public void goToNextStep(){
        Pause = false;
        newStepAskedByPlayer = true;
    }

    // See StopButton in editor
    public void updateTotalStep(){ //on click on stop button
        gameData.totalStep -= nbStep;
        nbStep = 0;
    }

    // See SpeedButton in editor
    public void speedTimeStep(){
        timeStep = defaultTimeStep/5;
    }

    // See ContinueButton in editor
    public void setToDefaultTimeStep(){
        timeStep = defaultTimeStep;
    }
}