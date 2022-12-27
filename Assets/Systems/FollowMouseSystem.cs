using System;
using FYFY_plugins.PointerManager;
using UnityEngine;
using FYFY;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

public class FollowMouseSystem : FSystem
{
	private GameData gameData;

	public static EndGameManager instance;

	public Camera camera;
	public GameObject leftMenu;	
	
	private Family f_followMouse = FamilyManager.getFamily(new AllOfComponents(typeof(FollowMouse)));
	private RaycastHit _hit;
	private Ray _ray;
	private RectTransform _rectTransform;

	protected override void onStart()
	{
		base.onStart();
		Application.targetFrameRate = 60;
		_rectTransform = leftMenu.GetComponent<RectTransform>();
		gameData = GameObject.Find("GameData").GetComponent<GameData>();
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		_ray = camera.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(_ray, out _hit, 100.0f);
		if (_hit.distance > 0)
		{
			Vector3 pos = _hit.point;
			if (_hit.transform.gameObject.layer == 0)
			{
				pos = new Vector3((float)Math.Floor(pos.x + 0.5f), (float)Math.Floor(pos.y + 0.5f),
					(float)Math.Floor(pos.z + 0.5f));
			}
			else
			{
				pos = _hit.transform.gameObject.transform.position + _hit.normal;
			}

			Debug.DrawRay(_hit.point, _hit.normal * 2, Color.green);
			if (Input.mousePosition.x > _rectTransform.sizeDelta.x)
			{
				foreach (GameObject go in f_followMouse)
				{
					go.transform.position = pos * go.transform.localScale.x;
					if (Input.GetMouseButtonDown(0))
					{
						GameObject newGo =
							UnityEngine.Object.Instantiate(gameData.editorBlock, pos, Quaternion.identity);
						GameObjectManager.bind(newGo);
						newGo.transform.localScale = Vector3.one;
					}

					if (Input.GetMouseButtonDown(1))
					{
						if (_hit.transform.gameObject.layer != 0)
						{
							GameObject toDelete = _hit.transform.gameObject;
							GameObjectManager.unbind(toDelete);
							UnityEngine.Object.Destroy(toDelete);
						}
					}
				}
			}
		}
	}
}