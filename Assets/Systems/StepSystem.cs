using UnityEngine;
using FYFY;
using System.Threading.Tasks;
using UnityEngine.UI;

public class StepSystem : FSystem {

    private Family newEnd_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));
    private Family highlightedItems = FamilyManager.getFamily(new AllOfComponents(typeof(UIActionType), typeof(HighLight)));
    private Family visibleContainers = FamilyManager.getFamily(new AllOfComponents(typeof(CanvasRenderer), typeof(ScrollRect), typeof(AudioSource)), new AllOfProperties(PropertyMatcher.PROPERTY.ACTIVE_SELF)); 
    private float timeStepCpt;
	private static float timeStep = 1.5f;
	private GameData gameData;
	public StepSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		gameData.nbStep = 0;
		timeStepCpt = timeStep;
        newStep_f.addEntryCallback(onNewStep);
    }

    private async void onNewStep(GameObject go)
    {
        GameObjectManager.removeComponent(go.GetComponent<NewStep>());
        timeStepCpt = timeStep;
        gameData.nbStep--;
        Debug.Log("StepSystem End");
        if(gameData.nbStep == 0){
            Debug.Log("end");
            await Task.Delay((int)timeStep*1000);
            Debug.Log("step+1");
            foreach(GameObject highlightedGO in highlightedItems){
                if (highlightedGO.GetComponent<HighLight>() != null){
                    Debug.Log("remove");
                    GameObjectManager.removeComponent<HighLight>(highlightedGO);
                }
            }
        }
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {

		//Organize each steps
		if(gameData.nbStep > 0 && newEnd_f.Count == 0){
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

}