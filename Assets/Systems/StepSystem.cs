using UnityEngine;
using FYFY;
using System.Collections;

/// <summary>
/// Manage steps (automatic simulation or controled by player)
/// </summary>
public class StepSystem : FSystem {

    private Family newEnd_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));
	private Family playerScriptEnds = FamilyManager.getFamily(new NoneOfComponents(typeof(Moved)), new AnyOfTags("Player"));
    private Family movingAgents = FamilyManager.getFamily(new AllOfComponents(typeof(Moved)));
    private float timeStepCpt;
    private static float defaultTimeStep = 1.5f; 
	private static float timeStep = defaultTimeStep;
	private GameData gameData;
    private int nbStep;
    private bool newStepAskedByPlayer;

    protected override void onStart()
    {
        nbStep = 0;
        newStepAskedByPlayer = false;
        GameObject go = GameObject.Find("GameData");
        if (go != null)
            gameData = go.GetComponent<GameData>();
        timeStepCpt = timeStep;
        newStep_f.addEntryCallback(onNewStep);
        //reset nbstep on execution end
        playerScriptEnds.addExitCallback(delegate { nbStep = 0; });
    }

    // See ExecuteButton in editor (launch execution process by adding FirstStep)
    public void countNewExecution()
    {
        gameData.totalExecute++;
        timeStepCpt = timeStep;
        gameData.totalStep++;
        nbStep++;
    }


    private void onNewStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<NewStep>());  
        timeStepCpt = timeStep;
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
        foreach(GameObject movingAgent in movingAgents)
            GameObjectManager.removeComponent<Moved>(movingAgent);
        Pause = true;
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