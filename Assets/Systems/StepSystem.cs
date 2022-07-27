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

    private Family playingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private Family editingMode_f = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

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

        playingMode_f.addEntryCallback(delegate
        {
            // count a new execution
            gameData.totalExecute++;
            gameData.totalStep++;
            timeStepCpt = timeStep;
            nbStep++;
            Pause = false;
            setToDefaultTimeStep();
        });

        editingMode_f.addEntryCallback(delegate
        {
            Pause = true;
        });
    }

    private void onNewStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<NewStep>());  
        timeStepCpt = timeStep;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        //Organize each steps
        if (newEnd_f.Count == 0 && (playerHasNextAction() || timeStepCpt > 0))
        {
            //activate step
            if (timeStepCpt <= 0)
            {
                GameObjectManager.addComponent<NewStep>(MainLoop.instance.gameObject);
                gameData.totalStep++;
                nbStep++;
                if (newStepAskedByPlayer)
                {
                    newStepAskedByPlayer = false;
                    Pause = true;
                }
            }
            else
                timeStepCpt -= Time.deltaTime;
        }
        else // newEnd_f.Count > 0 || (!playerHasNextAction && timeStep CPt <=0)
        {
            MainLoop.instance.StartCoroutine(delayCheckEnd());
            Pause = true;
        }
    }

    private IEnumerator delayCheckEnd()
    {
        // wait a new possible current action
        yield return null;
        yield return null;
        // If there are still no actions => end playing mode
        if (!playerHasNextAction() || newEnd_f.Count > 0)
        {
            ModeManager.instance.setEditMode();
            // We save history if no end or win
            if (newEnd_f.Count <= 0 || newEnd_f.First().GetComponent<NewEnd>().endType == NewEnd.Win)
                UISystem.instance.saveHistory();
        }
        else
            Pause = false;
    }

    private bool playerHasNextAction(){
		CurrentAction act;
		foreach(GameObject go in currentActions){
			act = go.GetComponent<CurrentAction>();
			if(act.agent != null && act.agent.CompareTag("Player") && act.GetComponent<BaseElement>().next != null)
				return true;
		}
        return false;
    }

    // See PauseButton, ContinueButton in editor
    public void autoExecuteStep(bool on){
        Pause = !on;
    }

    // See NextStepButton in editor
    public void goToNextStep(){
        Pause = false;
        newStepAskedByPlayer = true;
    }

    // See StopButton in editor
    public void cancelTotalStep(){ //on click on stop button
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