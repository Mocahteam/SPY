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

    private Family f_userExecutor = FamilyManager.getFamily(new AllOfComponents(typeof(ToggleGroup)), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family f_newCurrentAction = FamilyManager.getFamily(new AllOfComponents(typeof(CurrentAction), typeof(BasicAction)));

    private Family f_playingMode = FamilyManager.getFamily(new AllOfComponents(typeof(PlayMode)));

    public static TraceExecutionSystem instance;

    public TraceExecutionSystem()
    {
        instance = this;
    }

    protected override void onStart()
    {
        f_newCurrentAction.addEntryCallback(checkValidity);
        f_playingMode.addEntryCallback(delegate {
            setToggleState(true);
            resetTogglesNotifications();
            unselectAllToggles();
        });
        Pause = true;
    }

    public void onActionSelected()
    {
        // identifier les actions sélectionnées par l'utilisateur
        List<Toggle> selectedActions = new List<Toggle>();
        foreach (GameObject executor in f_userExecutor)
            foreach (Toggle action in executor.GetComponentsInChildren<Toggle>())
                if (action.isOn)
                    selectedActions.Add(action);
        // si on a au moins une action sélectionnée pour chaque robot contrôlé par le joueur, on peut passer à l'étape suivante
        if (selectedActions.Count >= f_userExecutor.Count && f_userExecutor.Count > 0)
        {
            GameObjectManager.addComponent<NewStep>(f_userExecutor.First());
            MainLoop.instance.StartCoroutine(waitAndClearUI());
        }
    }

    private IEnumerator waitAndClearUI()
    {
        // désactiver les boutons d'action pour éviter que l'utilisateur ne change d'avis pendant l'exécution
        setToggleState(false);

        yield return new WaitForSeconds(1f);
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

    private void checkValidity(GameObject currentAction)
    {
        // On ne traite les CurrentAction qu'en mode play et que si l'utilisateur contrôle l'execution des robots
        if (f_playingMode.Count > 0 && f_userExecutor.Count > 0)
        {
            CurrentAction ca = currentAction.GetComponent<CurrentAction>();
            // On ne vérifier la validité que pour les agents contrôlés par le joueur et donc pas les ennemis
            if (ca.agent.tag == "Player")
            {
                List<Toggle> toggles = ca.agent.GetComponent<ScriptRef>().executablePanel.GetComponentInChildren<ToggleGroup>(true).ActiveToggles().ToList();
                if (toggles.Count() == 1)
                {
                    Toggle enabledToggle = toggles[0];
                    if (ca.GetComponent<BasicAction>().actionType != enabledToggle.GetComponent<BasicAction>().actionType)
                    {
                        GameObjectManager.addComponent<NewEnd>(MainLoop.instance.gameObject, new { endType = NewEnd.WrongActionChosen });
                        GameObjectManager.setGameObjectState(enabledToggle.transform.Find("true").gameObject, false);
                        GameObjectManager.setGameObjectState(enabledToggle.transform.Find("false").gameObject, true);
                    }
                    else
                    {
                        GameObjectManager.setGameObjectState(enabledToggle.transform.Find("true").gameObject, true);
                        GameObjectManager.setGameObjectState(enabledToggle.transform.Find("false").gameObject, false);
                    }
                }
            }
        }
    }
}