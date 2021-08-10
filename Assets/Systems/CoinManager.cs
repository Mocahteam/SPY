using UnityEngine;
using FYFY;
using System.Collections;
using FYFY_plugins.TriggerManager;

/// <summary>
/// Manage collision between player agents and Coins
/// </summary>
public class CoinManager : FSystem {
    private Family robotcollision_f = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player"));
	private GameData gameData;
    private bool activeCoin;

	public CoinManager(){
		if (Application.isPlaying)
		{
			activeCoin = true;
			gameData = GameObject.Find("GameData").GetComponent<GameData>();
			robotcollision_f.addEntryCallback(onNewCollision);
		}
    }

    private void onNewCollision(GameObject robot){
		if(activeCoin){
			Triggered3D trigger = robot.GetComponent<Triggered3D>();
			foreach(GameObject target in trigger.Targets){
				//Check if the player collide with a coin
                if(target.CompareTag("Coin")){
                    gameData.totalCoin++;
                    target.GetComponent<AudioSource>().Play();
                    MainLoop.instance.StartCoroutine(coinDestroy(target));					
				}
			}			
		}
    }

	// See ExecuteButton, StopButton and ReloadState buttons in editor
	public void detectCollision(bool on){
		activeCoin = on;
	}

	private IEnumerator coinDestroy(GameObject go){
		go.GetComponent<ParticleSystem>().Play();
		go.GetComponent<Renderer>().enabled = false;
		yield return new WaitForSeconds(1f); // let time for animation
		GameObjectManager.setGameObjectState(go, false); // then disabling GameObject
	}
}