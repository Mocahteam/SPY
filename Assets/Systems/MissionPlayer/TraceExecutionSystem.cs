using FYFY;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manage Toggle Action that enables players to choice the next action that would be executed
/// </summary>
public class TraceExecutionSystem : FSystem {

    private Family f_userExecutor = FamilyManager.getFamily(new AllOfComponents(typeof(ToggleGroup)));
    private Family f_enabledUserExecutor = FamilyManager.getFamily(new AllOfComponents(typeof(ToggleGroup)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));

    private Family f_executablePanels = FamilyManager.getFamily(new AllOfComponents(typeof(ExecutablePanel)));
    private Family f_playMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));
    private Family f_editMode = FamilyManager.getFamily(new AllOfComponents(typeof(EditMode)));
    private GameData gameData;

    public static TraceExecutionSystem instance;

    public TraceExecutionSystem()
    {
        instance = this;
    }

    protected override void onStart()
    {
        GameObject go = GameObject.Find("GameData");
        if (go != null)
            gameData = go.GetComponent<GameData>();
        f_playMode.addEntryCallback(delegate {
            setToggleState(true);
            resetTogglesNotifications();
            unselectAllToggles();
            // en mode Play, définir si on doit afficher ou pas le panneau de sélection de la prochaine action à exécuter selon si l'utilisateur est autorisé à le faire
            foreach (GameObject exec_go in f_executablePanels)
            {
                // Définir si on doit afficher ou pas le panneau de sélection de la prochaine action à exécuter selon si l'utilisateur est autorisé à le faire et si l'agent est un robot contrôlé par le joueur ou un drone (ennemi)
                if (gameData.userExecutor && exec_go.GetComponentInChildren<LinkedWith>(true).target.tag == "Player")
                    GameObjectManager.setGameObjectState(exec_go.GetComponentInChildren<ToggleGroup>(true).gameObject, true);
                else
                    GameObjectManager.setGameObjectState(exec_go.GetComponentInChildren<ToggleGroup>(true).gameObject, false);
            }
        });
        f_editMode.addEntryCallback(delegate {
            // en mode Edit, toujours cacher les panneau de sélection de la prochaine action à exécuter
            foreach (GameObject exec_go in f_executablePanels)
                GameObjectManager.setGameObjectState(exec_go.GetComponentInChildren<ToggleGroup>(true).gameObject, false);
        });
        Pause = true;
    }

    public void onActionSelected()
    {
        // identifier les actions sélectionnées par l'utilisateur
        List<Toggle> selectedActions = new List<Toggle>();
        foreach (GameObject executor in f_enabledUserExecutor)
            foreach (Toggle action in executor.GetComponentsInChildren<Toggle>())
                if (action.isOn)
                    selectedActions.Add(action);
        // si on a au moins une action sélectionnée pour chaque robot contrôlé par le joueur, on peut passer à l'étape suivante
        if (selectedActions.Count >= f_enabledUserExecutor.Count && f_enabledUserExecutor.Count > 0)
        {
            GameObjectManager.addComponent<NewStep>(f_enabledUserExecutor.First());
            MainLoop.instance.StartCoroutine(waitAndClearUI());
        }
    }

    private IEnumerator waitAndClearUI()
    {
        yield return null; // Pour attendre que le NewStep soit pris en compte par le StepSystem et que le startStepTime soit mis à jour
        // désactiver les boutons d'action pour éviter que l'utilisateur ne change d'avis pendant l'exécution
        setToggleState(false);

        // Attendre 2 frame que les nouveau CurrentAction soient positionnés
        yield return null;
        yield return null;
        // Vérifier la validité des actions sélectionnées par l'utilisateur et affichage les notifications correspondantes (true/false) sur les boutons d'action
        checkValidity();


        // Attendre que l'on ait atteint 90% d'un pas de simulation
        yield return new WaitUntil(() => Time.time - gameData.startStepTime >= 0.90f / gameData.gameSpeed_current);

        // réinitialiser les boutons d'action pour la prochaine étape
        resetTogglesNotifications();
        // désélectionner tous les boutons d'action pour la prochaine étape
        unselectAllToggles();
        // réactiver les boutons d'action pour permettre à l'utilisateur de choisir la prochaine action
        setToggleState(true);
    }

    private void setToggleState(bool state)
    {
        foreach (GameObject executor in f_userExecutor)
            foreach (Toggle action in executor.GetComponentsInChildren<Toggle>())
                action.interactable = state;
    }

    private void resetTogglesNotifications()
    {
        foreach (GameObject executor in f_userExecutor)
            foreach (Toggle action in executor.GetComponentsInChildren<Toggle>())
            {
                GameObjectManager.setGameObjectState(action.transform.Find("true").gameObject, false);
                GameObjectManager.setGameObjectState(action.transform.Find("false").gameObject, false);
            }
    }

    private void unselectAllToggles()
    {
        foreach (GameObject executor in f_userExecutor)
            foreach (Toggle action in executor.GetComponentsInChildren<Toggle>())
                action.isOn = false;
    }

    private void checkValidity()
    {
        // On ne procède à la vérification qu'en mode play et que si l'utilisateur contrôle l'execution des robots
        if (f_playMode.Count > 0 && gameData.userExecutor)
        {
            foreach (GameObject executor in f_enabledUserExecutor)
            {
                List<Toggle> toggles = executor.GetComponent<ToggleGroup>().ActiveToggles().ToList();
                if (toggles.Count() == 1)
                {
                    Toggle enabledToggle = toggles[0];
                    // récupération de l'action courante associées à ce userExecutor
                    CurrentAction ca = executor.transform.parent.GetComponentInChildren<CurrentAction>(true);
                    if ((ca == null && enabledToggle.GetComponent<BasicAction>().actionType != BasicAction.ActionType.Wait) || (ca.GetComponent<BasicAction>().actionType != enabledToggle.GetComponent<BasicAction>().actionType))
                    {
                        MainLoop.instance.StartCoroutine(delayNewEnd());
                        GameObjectManager.setGameObjectState(enabledToggle.transform.Find("true").gameObject, false);
                        GameObjectManager.setGameObjectState(enabledToggle.transform.Find("false").gameObject, true);
                        GameObjectManager.addComponent<ActionPerformedForLRS>(enabledToggle.gameObject, new
                        {
                            verb = "traced",
                            objectType = "block",
                            result = true,
                            success = -1,
                            activityExtensions = new Dictionary<string, string>() {

                                { "value", ca == null ? "None" : ca.GetComponent<BasicAction>().actionType.ToString() },
                                { "error", enabledToggle.GetComponent<BasicAction>().actionType.ToString() }

                            }
                        });
                    }
                    else
                    {
                        GameObjectManager.setGameObjectState(enabledToggle.transform.Find("true").gameObject, true);
                        GameObjectManager.setGameObjectState(enabledToggle.transform.Find("false").gameObject, false);
                        GameObjectManager.addComponent<ActionPerformedForLRS>(enabledToggle.gameObject, new
                        {
                            verb = "traced",
                            objectType = "block",
                            result = true,
                            success = 1,
                            activityExtensions = new Dictionary<string, string>() {
                                { "value", enabledToggle.GetComponent<BasicAction>().actionType.ToString() }
                            }
                        });
                    }
                }
            }
        }
    }
    private IEnumerator delayNewEnd()
    {
        // Attendre que l'on ait atteint 80% d'un pas de simulation
        yield return new WaitUntil(() => Time.time - gameData.startStepTime >= 0.8f / gameData.gameSpeed_current);
        GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.WrongActionChosen });
    }
}