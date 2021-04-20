using UnityEngine;
using FYFY;
using System.Threading.Tasks;
using UnityEngine.UI;

public class StepSystem : FSystem {

    private Family newEnd_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family highlightedItems = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType), typeof(CurrentAction)));
    private Family visibleContainers = FamilyManager.getFamily(new AllOfComponents(typeof(CanvasRenderer), typeof(ScrollRect), typeof(AudioSource)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); 
	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef),typeof(Position)), new AnyOfTags("Player"));
    private float timeStepCpt;
	private static float timeStep = 1.5f;
	private GameData gameData;
	public StepSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		gameData.nbStep = 0;
		timeStepCpt = timeStep;
        newStep_f.addEntryCallback(onNewStep);
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

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {

		//Organize each steps
		//if(gameData.nbStep > 0 && newEnd_f.Count == 0){
		if(continueSteps() && newEnd_f.Count == 0){
            //activate step
            if (timeStepCpt <= 0)
            {
                GameObjectManager.addComponent<NewStep>(MainLoop.instance.gameObject);
                gameData.totalStep++;
            }
            else
                timeStepCpt -= Time.deltaTime;
		}
	}

    private bool continueSteps(){
        bool currentAction = false;
        foreach(GameObject robot in playerGO){
            if(robot.GetComponent<ScriptRef>().container.GetComponentInChildren<CurrentAction>()){
                currentAction = true;
                break;
            }  
        }
        return currentAction;
    }
    
    private void highlight(){
        /*
        foreach(GameObject go in highlightedItems){
            if(go != null){
                Debug.Log("highlight");
                GameObjectManager.removeComponent(go.GetComponent<HighLight>());
                if(go.GetComponent<BasicAction>() && go.GetComponent<BasicAction>().next != null){
                    GameObjectManager.addComponent<HighLight>(go.GetComponent<BasicAction>().next);
                }
                else if(go.GetComponent<IfAction>()){
                    if(go.GetComponent<IfAction>().firstChild != null)
                        GameObjectManager.addComponent<HighLight>(go.GetComponent<IfAction>().firstChild);
                    else if(go.GetComponent<IfAction>().next != null)
                        GameObjectManager.addComponent<HighLight>(go.GetComponent<IfAction>().next);
                }
                else if (go.GetComponent<ForAction>()){ //TO DO for children loop
                    if(go.GetComponent<ForAction>().firstChild != null)
                        GameObjectManager.addComponent<HighLight>(go.GetComponent<ForAction>().firstChild);
                    else if(go.GetComponent<ForAction>().next != null)
                        GameObjectManager.addComponent<HighLight>(go.GetComponent<ForAction>().next);
                }
            }                
        }*/
        /*
        GameObject container;
        foreach(GameObject robot in playerGO){
            container = robot.GetComponent<ScriptRef>().container.transform.Find("Viewport").Find("ScriptContainer").gameObject;
            //
        }*/        
    }

}