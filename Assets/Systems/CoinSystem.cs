using UnityEngine;
using FYFY;
using System.Collections;

public class CoinSystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(Script)), new AnyOfTags("Player"));
	private Family coinGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Coin"));
	private float speed = 20f;
	private GameData gameData;

	public CoinSystem(){
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
	}
	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		//Check if recolted
		if(gameData.checkStep){
			foreach(GameObject player in playerGO){
				foreach(GameObject coin in coinGO){
					if(player.GetComponent<Position>().x == coin.GetComponent<Position>().x &&  player.GetComponent<Position>().z == coin.GetComponent<Position>().z){
						gameData.totalCoin++;
						coin.GetComponent<AudioSource>().Play();
						MainLoop.instance.StartCoroutine(coinDestroy(coin));

					}
				}
			}
		}

		//Rotation of the coin
		foreach(GameObject coin in coinGO){
			coin.transform.Rotate(Vector3.forward * Time.deltaTime * speed);
		}
	}

	private IEnumerator coinDestroy(GameObject go){

		go.GetComponent<ParticleSystem>().Play();
		go.GetComponent<Renderer>().enabled = false;

		yield return new WaitForSeconds(1f);
		GameObjectManager.unbind(go);
		Object.Destroy(go);
	}
}