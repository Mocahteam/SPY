using FYFY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage steps (automatic simulation or controled by player)
/// </summary>
/// 
/*La boucle de simulation orchestre plusieurs systèmes
 * Le StepSystem genère à temps constant le composant NewStep, soit la génération de ce composant à l'instant T
 * en T' (début du lateUpdate) dans le CurrentActionManager, le composant NewStep déclenche la suppression des currentActions et demande d'ajouter en T+1 des nouvelles currentActions (coroutines)
 * en T+1 (phase d'update) ajout des currentActions par les coroutines lancées par le CurrentActionManager
 * en T+1' (début du lateUpdate) le CurrentActionExecutor dépile les nouvelles CurrentActions et s'auto réveille pour corriger les déplacement en cas de prédiction de collisions
 * en T+1'' (phase de lateUpdate) le CurrentActionExecutor corrige les positions à atteindre en fonction des collisions prédites et informe les systèmes dépendants que tout est ok avec le composant PositionCorrected
 *      A noter que l'exécution des actions Activate ajoutent des composant Triggered
 * en post T+2 sur l'écoute du composant PositionCorrected, les callback du MoveSystem et du DetectorManager activent leurs onProcess et le DoorAndConsoleManager dépile les Triggered et synchronise les portes => les portes sont donc à jour quand le DetectorManager va processer
 * en T+2 (phase d'update) le MoveSystem et le DetectorManager font leur job (onProcess)
 * ...
 * Les collisions arrivent avant le déclenchement du STEP suivant se qui peur déclencher des NewEnd
 * ...
 * en T+X => quand la durée d'un step est dépassé, génération d'un nouveau NewStep
 */
public class StepSystem : FSystem {

    private Family f_newEnd = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
    private Family f_newStep = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family f_currentActions = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction)));

    private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private Family f_editingMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));

    private Family f_executablePanels = FamilyManager.getFamily(new AnyOfTags("ScriptConstructor"), new AllOfComponents(typeof(UIRootExecutor)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_door = FamilyManager.getFamily(new AllOfComponents(typeof(ActivationSlot), typeof(Position), typeof(Animator)), new AnyOfTags("Door"));

    private GameData gameData;
    private int nbStep;
    private bool newStepAskedByPlayer;
    private bool needPause;

    public RectTransform editableContainers;

    protected override void onStart()
    {
        nbStep = 0;
        newStepAskedByPlayer = false;
        needPause = false;
        GameObject go = GameObject.Find("GameData");
        if (go != null)
            gameData = go.GetComponent<GameData>();
        f_newStep.addEntryCallback(onNewStep);

        f_playingMode.addEntryCallback(delegate
        {
            // count a new execution
            gameData.totalExecute++;
            setToDefaultTimeStep();

            gameData.totalStep++;
            gameData.startStepTime = Time.time;
            nbStep++;

            Pause = false;
        });

        f_editingMode.addEntryCallback(delegate
        {
            Pause = true;
        });

        Pause = true;
    }

    private void onNewStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<NewStep>());
        gameData.startStepTime = Time.time;
        gameData.totalStep++;
        nbStep++;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        // Attendre que le temps d'un pas de simulation soit écoulé
        if (Time.time - gameData.startStepTime >= 1 / gameData.gameSpeed_current)
        {
            // Une fois qu'on a bien attendu que le temps était écoulé, on vérifie si une fin ne se serait pas déclenchée avant de chercher à lancer un nouveau Step
            if (f_newEnd.Count == 0)
            {
                // Si le joueur a demandé de mettre en pause
                if (needPause)
                {
                    needPause = false;
                    Pause = true;

                    string scriptsContent = "";
                    foreach (GameObject executablePanel in f_executablePanels)
                        scriptsContent += exportExecutableScriptToString(executablePanel.transform);

                    GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
                    {
                        verb = "paused",
                        objectType = "program",
                        activityExtensions = new Dictionary<string, string>() {
                            { "context", scriptsContent }
                        }
                    });
                }
                // Si aucun robot n'a d'action suivante, revenir en mode éditeur
                else if (!playerHasNextAction())
                {
                    GameObjectManager.addComponent<EditMode>(MainLoop.instance.gameObject);
                    GameObjectManager.addComponent<AskToSaveHistory>(MainLoop.instance.gameObject);
                    needPause = false;
                    Pause = true;
                }

                // Cas général, on demande un nouveau pas de simulation
                else
                {
                    // start a new Step
                    GameObjectManager.addComponent<NewStep>(MainLoop.instance.gameObject);

                    if (newStepAskedByPlayer)
                    {
                        MainLoop.instance.StartCoroutine(delaySteppedStatement());
                        newStepAskedByPlayer = false;
                        Pause = true;
                    }
                }
            }
        }
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

        if (on)
        {
            GameObjectManager.addComponent<ActionPerformedForLRS>(MainLoop.instance.gameObject, new
            {
                verb = "resumed",
                objectType = "program"
            });
            Pause = false;
        }
        else
        {
            needPause = true;
        }
    }

    private string exportExecutableScriptToString(Transform scriptContainer)
    {
        string scriptsContent = scriptContainer.GetComponentInChildren<UIRootExecutor>(true).scriptName + " {";
        for (int i = 0; i < scriptContainer.childCount; i++)
            scriptsContent += " " + UtilityGame.exportBlockToString(scriptContainer.GetChild(i).GetComponent<Highlightable>());
        scriptsContent += " }\n";
        return scriptsContent;
    }

    // See NextStepButton in editor
    public void goToNextStep(){
        if (Time.time - gameData.startStepTime >= 1 / gameData.gameSpeed_current)
        {
            Pause = false;
            newStepAskedByPlayer = true;
        }
    }

    private IEnumerator delaySteppedStatement()
    {
        // On attend une frame que les nouvelles CurrentActions soient définies
        yield return null;
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
        syncDoorSpeedAnimation();
    }

    // See ContinueButton in editor
    public void setToDefaultTimeStep(){
        gameData.gameSpeed_current = gameData.gameSpeed_default;
        syncDoorSpeedAnimation();
    }

    private void syncDoorSpeedAnimation()
    {
        foreach (GameObject door in f_door)
            door.GetComponent<Animator>().speed = gameData.gameSpeed_current;
    }
}