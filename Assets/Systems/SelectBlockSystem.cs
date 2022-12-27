using UnityEngine;
using FYFY;

public class SelectBlockSystem : FSystem
{
	private GameData gameData;
	public GameData prefabGameData;
	
	public static SelectBlockSystem instance;
	public GameObject mover;

	public SelectBlockSystem()
	{
		instance = this;
	}
	
	protected override void onStart()
	{
		base.onStart();
		if (!GameObject.Find("GameData"))
		{
			gameData = UnityEngine.Object.Instantiate(prefabGameData);
			gameData.name = "GameData";
			GameObjectManager.dontDestroyOnLoadAndRebind(gameData.gameObject);
		}
		else
		{
			gameData = GameObject.Find("GameData").GetComponent<GameData>();
		}
	}

	public void selectBlock(GameObject obj)
	{
		GameObjectManager.unbind(mover.transform.GetChild(0).gameObject);
		UnityEngine.GameObject.Destroy(mover.transform.GetChild(0).gameObject);
		GameObject newGO = UnityEngine.GameObject.Instantiate(obj, mover.transform);
		newGO.GetComponent<BoxCollider>().enabled = false;
		newGO.transform.localScale /= 3;
		newGO.transform.localPosition = Vector3.zero;
		GameObjectManager.bind(newGO);
		gameData.editorBlock = obj;
	}
}


