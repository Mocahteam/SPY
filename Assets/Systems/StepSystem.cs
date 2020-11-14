using UnityEngine;
using FYFY;

public class StepSystem : FSystem {


	private float timeStepCpt;
	private float timeStep = 1f;
	private GameData gameData;
	public StepSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		gameData.step = false;
		gameData.checkStep = false;
		gameData.generateStep = false;
		gameData.nbStep = 0;
		gameData.initialize = true;
		timeStepCpt = timeStep;
		gameData.endLevel = 0;
	} 

	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		if(gameData.initialize){
			gameData.initialize = false;
		}

		if(gameData.nbStep > 0 && gameData.endLevel == 0){
			if(gameData.checkStep){
				timeStepCpt = timeStep;
				gameData.checkStep = false;
				gameData.nbStep--;
			}
			else if(gameData.generateStep){
				gameData.generateStep = false;
				gameData.checkStep = true;
			}
			else if(gameData.step){
				gameData.step = false;
				gameData.generateStep = true;
			}
			else if(timeStepCpt <= 0){
				gameData.step = true;
			}
			else{
				timeStepCpt -= Time.deltaTime;
			}
		}
		else{
			gameData.step = false;
			gameData.checkStep = false;
			gameData.generateStep = false;
		}
	}
}