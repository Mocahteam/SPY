using FYFY;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manage steps (automatic simulation or controled by player)
/// </summary>
public class StepSystem : FSystem {

    private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
    private Family f_newStep = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));

    private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

    private Family f_executablePanels = FamilyManager.getFamily(new AnyOfTags("ScriptConstructor"), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private float timeStepCpt;
	private GameData gameData;
    private int nbStep;
    private bool newStepAskedByPlayer;

    public RectTransform editableContainers;

    protected override void onStart()
    {
        nbStep = 0;
        newStepAskedByPlayer = false;
        GameObject go = GameObject.Find("GameData");
        if (go != null)
        {
            gameData = go.GetComponent<GameData>();
            timeStepCpt = 1 / gameData.gameSpeed_current;
        }
        f_newStep.addEntryCallback(onNewStep);

        f_playingMode.addEntryCallback(delegate
        {
            // count a new execution
            gameData.totalExecute++;
            gameData.totalStep++;
            timeStepCpt = 1 / gameData.gameSpeed_current;
            nbStep++;
            Pause = false;
            setToDefaultTimeStep();
        });

        f_editingMode.addEntryCallback(delegate
        {
            Pause = true;
        });
    }

    private void onNewStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<NewStep>());  
        timeStepCpt = (1 / gameData.gameSpeed_current) + timeStepCpt; // le "+ timeStepCpt" permet de prendre en compte le débordement de temps de la frame précédente
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        //Organize each steps
        if (f_newEnd.Count == 0 && playerHasNextAction()) // pas de fin et encore au moins une action à exécuter
        {
            if (timeStepCpt <= 0 || newStepAskedByPlayer) // temps d'un pas de simulation écoulé ou nouvelle action demandée par le joueur
            {
                // new step
                GameObjectManager.addComponent<NewStep>(MainLoop.instance.gameObject);
                gameData.totalStep++;
                nbStep++;
                if (newStepAskedByPlayer)
                {
                    newStepAskedByPlayer = false;
                    Pause = true;
                }
            }
            else // temps d'un pas non encore écoulé et pas d'action demandée par le joueur
                timeStepCpt -= Time.deltaTime;
        }
        else // fin détectée ou plus d'action à exécuter
        {
            if (timeStepCpt > 0)
                timeStepCpt -= Time.deltaTime;
            else
            {
                MainLoop.instance.StartCoroutine(delayCheckEnd());
                Pause = true;
            }
        }
    }

    private IEnumerator delayCheckEnd()
    {
        // wait a new possible current action
        yield return null;
        yield return null;
        // If there are still no actions => return to edit mode
        if (!playerHasNextAction() || f_newEnd.Count > 0)
        {
            GameObjectManager.addComponent<EditMode>(MainLoop.instance.gameObject);
            // We save history if no end or win
            if (f_newEnd.Count <= 0)
                GameObjectManager.addComponent<AskToSaveHistory>(MainLoop.instance.gameObject);
        }
        else
            Pause = false;
    }

    // Check if one of the robot programmed by the player has a next action to perform
    private bool playerHasNextAction(){
		CurrentAction act;
		foreach(GameObject go in f_currentActions){
			act = go.GetComponent<CurrentAction>();
			if(act.agent != null && act.agent.CompareTag("Player") && act.GetComponent<BaseElement>().next != null)
				return true;
		}
        return false;
    }

    // See PauseButton, ContinueButton in editor
    public void autoExecuteStep(bool on){
        Pause = !on;

        if (Pause)
        {
            string scriptsContent = "";
            foreach (GameObject executablePanel in f_executablePanels)
                scriptsContent += exportExecutableScriptToString(executablePanel.transform);

            GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
            {
                verb ="paused",
                objectType = "program",
                activityExtensions = new Dictionary<string, string>() {
                    { "context", scriptsContent }
                }
            });
        } else
        {
            GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
            {
                verb = "resumed",
                objectType = "program"
            });
        }
    }

    private string exportExecutableScriptToString(Transform scriptContainer)
    {
        string scriptsContent = scriptContainer.parent.parent.parent.Find("Header").Find("agentName").GetComponent<TMP_InputField>().text + " {";
        for (int i = 0; i < scriptContainer.childCount; i++)
            scriptsContent += " " + Utility.exportBlockToString(scriptContainer.GetChild(i).GetComponent<Highlightable>());
        scriptsContent += " }\n";
        return scriptsContent;
    }

    // See NextStepButton in editor
    public void goToNextStep(){
        Pause = false;
        newStepAskedByPlayer = true;
        MainLoop.instance.StartCoroutine(delayStatement());
    }

    private IEnumerator delayStatement()
    {
        yield return new WaitForSeconds(.25f);
        string scriptsContent = "";
        foreach (GameObject executablePanel in f_executablePanels)
            scriptsContent += exportExecutableScriptToString(executablePanel.transform);
        GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
        {
            verb = "stepped",
            objectType = "program",
            activityExtensions = new Dictionary<string, string>() {
                { "context", scriptsContent }
            }
        });
    }

    // See StopButton in editor
    public void cancelTotalStep(){ //click on stop button
        gameData.totalStep -= nbStep;
        gameData.totalExecute--;
        nbStep = 0;
    }

    // See SpeedButton in editor
    public void speedTimeStep(){
        gameData.gameSpeed_current = gameData.gameSpeed_default * 3f;
    }

    // See ContinueButton in editor
    public void setToDefaultTimeStep(){
        gameData.gameSpeed_current = gameData.gameSpeed_default;
    }

    public void startStepImmediate()
    {
        timeStepCpt = 0.1f; // To quickly start the next step
    }
}