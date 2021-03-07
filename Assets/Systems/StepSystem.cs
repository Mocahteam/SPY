using UnityEngine;
using FYFY;

public class StepSystem : FSystem {

    private Family newEnd_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewEnd)));
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));

    private float timeStepCpt;
	private float timeStep = 1.5f;
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
        timeStepCpt = timeStep;
        gameData.nbStep--;
        Debug.Log("StepSystem End");
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