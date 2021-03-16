using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using UnityEngine.UI;

public class HighLightSystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family highlightedGO = FamilyManager.getFamily(new AllOfComponents(typeof(HighLight), typeof(PointerOver)));
    private Family newStep_f = FamilyManager.getFamily(new AllOfComponents(typeof(NewStep)));

	private Family enemyScriptContainer_f = FamilyManager.getFamily(new AllOfComponents(typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(Image)), new NoneOfComponents(typeof(UITypeContainer)), new AnyOfTags("ScriptConstructor"));

    private GameObject highLightedItem;
	private GameObject EnemyScriptContainer;

	private GameObject scriptInWindow;

	private GameData gameData;
	
	public HighLightSystem(){
		highlightedGO.addEntryCallback(highLightItem);
		highlightedGO.addExitCallback(unHighLightItem);
        newStep_f.addEntryCallback(onNewStep);
        highLightedItem = null;
		scriptInWindow = null;
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
		EnemyScriptContainer = enemyScriptContainer_f.First();
	}
	
    private void onNewStep(GameObject unused)
    {
        //Change the higlighted action every step
        if (scriptInWindow){
            foreach (Transform child in EnemyScriptContainer.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            ActionManipulator.ScriptToContainer(scriptInWindow.GetComponent<Script>(), EnemyScriptContainer);
        }
    }

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

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