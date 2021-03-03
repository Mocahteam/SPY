using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;

public class HighLightSystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family highlightedGO = FamilyManager.getFamily(new AllOfComponents(typeof(HighLight), typeof(PointerOver)));

	private GameObject highLightedItem;
	private GameObject EnemyScriptContainer;

	private GameObject scriptInWindow;

	private GameData gameData;
	
	public HighLightSystem(){
		highlightedGO.addEntryCallback(highLightItem);
		highlightedGO.addExitCallback(unHighLightItem);
		highLightedItem = null;
		scriptInWindow = null;
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		EnemyScriptContainer = GameObject.Find("EnemyScript").transform.GetChild(0).transform.GetChild(0).gameObject;	
	}
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		//Change the higlighted action every step
		if(gameData.step && scriptInWindow){
			foreach (Transform child in EnemyScriptContainer.transform) {
				GameObject.Destroy(child.gameObject);
			}
			ActionManipulator.ScriptToContainer(scriptInWindow.GetComponent<Script>(), EnemyScriptContainer);
		}
		
		//If click on highlighted item and item has a script, then show script in the 2nd script window
		if(highLightedItem && Input.GetMouseButtonDown(0) && highLightedItem.GetComponent<Script>()){
			foreach (Transform child in EnemyScriptContainer.transform) {
				GameObject.Destroy(child.gameObject);
			}
			scriptInWindow =  highLightedItem;
			GameObject.Find("EnemyScript").GetComponent<AudioSource>().Play();
			ActionManipulator.ScriptToContainer(highLightedItem.GetComponent<Script>(), EnemyScriptContainer);
		}

	}

	public void highLightItem(GameObject go){
		highLightedItem = go;

		if(go.GetComponent<Renderer>()){
			go.GetComponent<HighLight>().basecolor = go.GetComponent<Renderer>().material.color;
			go.GetComponent<Renderer>().material.color = Color.yellow;
		}
		else if(go.transform.childCount > 0 && go.transform.GetChild(0).GetComponent<Renderer>()){
			go.GetComponent<HighLight>().basecolor = go.transform.GetChild(0).GetComponent<Renderer>().material.color;
			go.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.yellow;
		}
		else if(go.transform.childCount > 0 && go.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>()){
			go.GetComponent<HighLight>().basecolor = go.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material.color;
			go.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material.color = Color.yellow;
		}
	}

	public void unHighLightItem(int id){
		if(highLightedItem != null){
			if(highLightedItem.GetComponent<Renderer>()){
				highLightedItem.GetComponent<Renderer>().material.color = highLightedItem.GetComponent<HighLight>().basecolor;
			}
			else if(highLightedItem.transform.childCount > 0 && highLightedItem.transform.GetChild(0).GetComponent<Renderer>()){
				highLightedItem.transform.GetChild(0).GetComponent<Renderer>().material.color = highLightedItem.GetComponent<HighLight>().basecolor;
			}
			else if(highLightedItem.transform.childCount > 0 && highLightedItem.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>()){
				highLightedItem.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().material.color = highLightedItem.GetComponent<HighLight>().basecolor;
			}
		}
			
		highLightedItem = null;
	}
}