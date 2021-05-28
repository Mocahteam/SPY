using UnityEngine;
using FYFY;
using System.Collections;
using FYFY_plugins.TriggerManager;

public class CoinSystem : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.

	private Family playerGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position)), new AnyOfTags("Player"));
    private Family coinGO = FamilyManager.getFamily(new AllOfComponents(typeof(Position), typeof(AudioSource), typeof(ParticleSystem)), new AnyOfTags("Coin"), new AnyOfProperties(PropertyMatcher.PROPERTY.ACTIVE_IN_HIERARCHY));
    private Family newStep_f = FamilyManager.getFamily(new AnyOfComponents(typeof(NewStep), typeof(FirstStep)));
    private Family robotcollision_f = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D)), new AnyOfTags("Player"));
    private float speed = 20f;
	private GameData gameData;
    private bool activeCoin;

	public CoinSystem(){
        activeCoin = true;
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
        //newStep_f.addEntryCallback(onNewStep);
        robotcollision_f.addEntryCallback(onNewCollision);
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

	public void detectCollision(bool on){
		activeCoin = on;
	}

/*

    private void onNewStep(GameObject unused)
    {
        foreach (GameObject player in playerGO)
        {
            foreach (GameObject coin in coinGO)
            {
                if (player.GetComponent<Position>().x == coin.GetComponent<Position>().x && player.GetComponent<Position>().z == coin.GetComponent<Position>().z)
                {
                    gameData.totalCoin++;
                    coin.GetComponent<AudioSource>().Play();
                    MainLoop.instance.StartCoroutine(coinDestroy(coin));
                }
            }
        }
    }
*/

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
		//Rotation of the coin
		foreach(GameObject coin in coinGO){
			coin.transform.Rotate(Vector3.forward * Time.deltaTime * speed);
		}
	}

	private IEnumerator coinDestroy(GameObject go){

		go.GetComponent<ParticleSystem>().Play();
		go.GetComponent<Renderer>().enabled = false;

		yield return new WaitForSeconds(1f);
		GameObjectManager.setGameObjectState(go, false);
        //GameObjectManager.unbind(go);
		//Object.Destroy(go);
	}
}