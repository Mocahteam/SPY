using UnityEngine;
using FYFY;
using FYFY_plugins.PointerManager;
using FYFY_plugins.TriggerManager;
using UnityEngine.UI;

public class CameraSystem : FSystem {
	//private Family cameraGO = FamilyManager.getFamily(new AllOfComponents(typeof(CameraComponent)));
	private Family cameraGO = FamilyManager.getFamily(new AllOfComponents(typeof(CameraComponent)), new AnyOfTags("MainCamera"));
	private Family agents = FamilyManager.getFamily(new AllOfComponents(typeof(ScriptRef)));

	//private Family UIGO = FamilyManager.getFamily(new AnyOfComponents(typeof(UIActionType), typeof(UITypeContainer), typeof(ElementToDrag)),
	//												new AllOfComponents(typeof(PointerOver)));

	//private Family playerGO = FamilyManager.getFamily(new AnyOfComponents(typeof(Script)), new AnyOfTags("Player"));
	
	private Family playerGO = FamilyManager.getFamily(new AnyOfTags("Player"));
	private Family UIfocused = FamilyManager.getFamily(new AllOfComponents(typeof(RectTransform), typeof(PointerOver))); 
	private GameData gameData;
	private Transform target;
	private float smoothSpeed = 0.125f;
	private Vector3 offset = new Vector3(6,15,0);

	public CameraSystem()
	{
		gameData = GameObject.Find("GameData").GetComponent<GameData>();

		foreach( GameObject go in cameraGO)
		{
			onGOEnter(go);
	    }
	    cameraGO.addEntryCallback(onGOEnter);

        foreach (GameObject player in playerGO)
        {
            setCameraOnGO(player);
        }

		agents.addEntryCallback(setLocateButtons);

    }

	private void onGOEnter(GameObject go)
	{
        go.GetComponent<CameraComponent>().orbitH = go.GetComponent<CameraComponent>().transform.eulerAngles.y;
        go.GetComponent<CameraComponent>().orbitV = go.GetComponent<CameraComponent>().transform.eulerAngles.x;
        go.GetComponent<CameraComponent>().init_X = go.transform.position.x;
        go.GetComponent<CameraComponent>().init_Y = go.transform.position.y;
        go.GetComponent<CameraComponent>().init_Z = go.transform.position.z;
        //go.GetComponent<CameraComponent>().init_X_angle = go.transform.rotation.x;
        //go.GetComponent<CameraComponent>().init_Y_angle = go.transform.rotation.y;
        go.GetComponent<CameraComponent>().initRotation = go.transform.localRotation.eulerAngles;
        //go.GetComponent<CameraComponent>().movementRotation = new Vector3 (Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {

		foreach( GameObject go in cameraGO ){
			if(target){
				go.transform.position = Vector3.MoveTowards(go.transform.position, (target.position+offset), smoothSpeed);
				//go.transform.position = Vector3.MoveTowards(go.transform.position, target.position+Vector3.Scale(target.position,Vector3.Scale(go.transform.localRotation.eulerAngles,offset)), smoothSpeed);
				go.transform.LookAt(target); 
			}
				
			
			/*
			// Pour placer la caméra derrière un agent
			//if (Input.GetMouseButtonDown(0))
			if (Input.GetKeyDown(KeyCode.Space))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
        		RaycastHit hit = new RaycastHit();
		        if (Physics.Raycast (ray, out hit))
		        {
		            Debug.Log(hit.collider.gameObject.name);
		            Debug.Log(hit.collider.transform.position);
		            go.transform.position = new Vector3(hit.collider.transform.position.x, go.transform.position.y, hit.collider.transform.position.z);
		            //go.transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * go.GetComponent<CameraComponent>().dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * go.GetComponent<CameraComponent>().dragSpeed, 0);
		        }
			}
			*/
			if(Input.anyKey){
				target = null;
			}
		    // gauche droite
		    go.transform.position += new Vector3(go.transform.forward.x + go.transform.up.x, 0, go.transform.forward.z + go.transform.up.z ) * Input.GetAxis("Vertical") * go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime;
			// avance recule
			go.transform.position += new Vector3(go.transform.right.x, 0, go.transform.right.z ) * Input.GetAxis("Horizontal") * go.GetComponent<CameraComponent>().cameraSpeed * Time.deltaTime;
			
			//Debug.Log("position x = " + go.transform.position.x);
			//Debug.Log("position y = " + go.transform.position.y);
			//Debug.Log("position z = " + go.transform.position.z);
			
			/*
			Debug.Log("rotation x = " + go.transform.rotation.x);
			Debug.Log("rotation y = " + go.transform.rotation.y);
			Debug.Log("rotation z = " + go.transform.rotation.z);
			*/
			
			// Limites de la caméra
			/*
			go.transform.position = new Vector3(
			   Mathf.Clamp(Camera.main.transform.position.x, go.GetComponent<CameraComponent>().MIN_X, go.GetComponent<CameraComponent>().MAX_X),
			   Mathf.Clamp(Camera.main.transform.position.y, go.GetComponent<CameraComponent>().MIN_Y, go.GetComponent<CameraComponent>().MAX_Y),
			   Mathf.Clamp(Camera.main.transform.position.z, go.GetComponent<CameraComponent>().MIN_Z, go.GetComponent<CameraComponent>().MAX_Z));
			*/
			
			float min_x = go.GetComponent<CameraComponent>().init_X+go.GetComponent<CameraComponent>().MIN_X;
			float max_x = go.GetComponent<CameraComponent>().init_X+go.GetComponent<CameraComponent>().MAX_X;
			float min_y = go.GetComponent<CameraComponent>().init_Y+go.GetComponent<CameraComponent>().MIN_Y;
			float max_y = go.GetComponent<CameraComponent>().init_Y+go.GetComponent<CameraComponent>().MAX_Y;
			float min_z = go.GetComponent<CameraComponent>().init_Z+go.GetComponent<CameraComponent>().MIN_Z;
			float max_z = go.GetComponent<CameraComponent>().init_Z+go.GetComponent<CameraComponent>().MAX_Z;
			go.transform.position = new Vector3(
			   Mathf.Clamp(Camera.main.transform.position.x, min_x, max_x),
			   Mathf.Clamp(Camera.main.transform.position.y, min_y, max_y),
			   Mathf.Clamp(Camera.main.transform.position.z, min_z, max_z));
			//------------------------------------------------------------------------------------

			// Déplacement avec la molette comme dans l'éditeur
	        if (Input.GetMouseButton(2))
            {
				target = null;
            	Cursor.lockState = CursorLockMode.Locked;
	        	Cursor.visible = false;
                go.transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * go.GetComponent<CameraComponent>().dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * go.GetComponent<CameraComponent>().dragSpeed, 0);
            	go.transform.position = new Vector3(
			   Mathf.Clamp(Camera.main.transform.position.x, min_x, max_x),
			   Mathf.Clamp(Camera.main.transform.position.y, min_y, max_y),
			   Mathf.Clamp(Camera.main.transform.position.z, min_z, max_z));
            }

			// Zoom avec la molette
			else if(Input.GetAxis("Mouse ScrollWheel") < 0 && UIfocused.Count == 0)
	        {
				target = null;
	            if(go.GetComponent<CameraComponent>().ScrollCount >= go.GetComponent<CameraComponent>().ScrollWheelminPush && go.GetComponent<CameraComponent>().ScrollCount < go.GetComponent<CameraComponent>().ScrollWheelLimit)
	            {
	                go.transform.position += new Vector3(0, go.GetComponent<CameraComponent>().zoomSpeed, 0);
	                go.GetComponent<CameraComponent>().ScrollCount++;
	            }
	        }
	        else if(Input.GetAxis("Mouse ScrollWheel") > 0 && UIfocused.Count == 0)
	        {
				target = null;
	            if(go.GetComponent<CameraComponent>().ScrollCount > go.GetComponent<CameraComponent>().ScrollWheelminPush && go.GetComponent<CameraComponent>().ScrollCount <= go.GetComponent<CameraComponent>().ScrollWheelLimit)
	            {
	                go.transform.position -= new Vector3(0, go.GetComponent<CameraComponent>().zoomSpeed, 0);
	                go.GetComponent<CameraComponent>().ScrollCount--;
	            }
	        }
	        
            // Déplacement de type orbite
            else if (Input.GetMouseButton(1))
            {
				target = null;
            	go.GetComponent<CameraComponent>().orbitH += go.GetComponent<CameraComponent>().lookSpeedH * Input.GetAxis("Mouse X");
                go.GetComponent<CameraComponent>().orbitV -= go.GetComponent<CameraComponent>().lookSpeedV * Input.GetAxis("Mouse Y");
                go.GetComponent<CameraComponent>().transform.eulerAngles = new Vector3(go.GetComponent<CameraComponent>().orbitV, go.GetComponent<CameraComponent>().orbitH, 0f);

                /*
            	Vector3 currentRotation = go.transform.localRotation.eulerAngles; // ne pas modifier
            	float minRotation_x = go.GetComponent<CameraComponent>().initRotation.x + go.GetComponent<CameraComponent>().MIN_X_angle;
				float maxRotation_x = go.GetComponent<CameraComponent>().initRotation.x + go.GetComponent<CameraComponent>().MAX_X_angle;
            	float minRotation_y = go.GetComponent<CameraComponent>().initRotation.y + go.GetComponent<CameraComponent>().MIN_Y_angle;
				float maxRotation_y = go.GetComponent<CameraComponent>().initRotation.y + go.GetComponent<CameraComponent>().MAX_Y_angle;
				//currentRotation.x = Mathf.Clamp(currentRotation.x, minRotation_x, maxRotation_x);
				//currentRotation.y = Mathf.Clamp(currentRotation.y, minRotation_y, maxRotation_y);
				currentRotation.x = Mathf.Clamp(go.GetComponent<CameraComponent>().orbitV, minRotation_x, maxRotation_x);
				currentRotation.y = Mathf.Clamp(go.GetComponent<CameraComponent>().orbitH, minRotation_y, maxRotation_y);
				go.transform.localRotation = Quaternion.Euler (currentRotation); // ne pas modifier
				*/
            }
	        else
	        {
	        	Cursor.lockState = CursorLockMode.None;
	        	Cursor.visible = true;
	    	}
			//------------------------------------------------------------------------------------
		}		
	}

	public void setCameraOnGO(GameObject go){
		target = go.transform;
		/*
		foreach(GameObject camera in cameraGO){
			Vector3 currentVector = camera.transform.position;
			Vector3 nextVector = new Vector3(go.transform.position.x +5, camera.transform.position.y, go.transform.position.z);
			camera.transform.position = Vector3.MoveTowards(currentVector, nextVector, Time.deltaTime);
		}
		*/
	}
	public void setLocateButtons(GameObject go){
		//foreach (GameObject go in agents){
		Debug.Log("setLocateButtons");
		go.GetComponent<ScriptRef>().uiContainer.transform.Find("Header").Find("locateButton").GetComponent<Button>().onClick.AddListener(delegate{setCameraOnGO(go);});
		//}
	}
}