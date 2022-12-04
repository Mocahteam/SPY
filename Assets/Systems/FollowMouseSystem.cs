using System;
using UnityEngine;
using FYFY;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class FollowMouseSystem : FSystem
{
	public static EndGameManager instance;

	public Camera camera;
	public GameObject prefab;
	
	private Family f_followMouse = FamilyManager.getFamily(new AllOfComponents(typeof(FollowMouse)));
	private RaycastHit _hit;
	private Ray _ray;

	protected override void onStart()
	{
		base.onStart();
		Application.targetFrameRate = 60;
	}

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		_ray = camera.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(_ray, out _hit);
		Vector3 pos = _hit.point;
		if (_hit.transform.gameObject.layer == 0)
		{
			pos = new Vector3((float)Math.Floor(pos.x + 0.5f), (float)Math.Floor(pos.y + 0.5f),
				(float)Math.Floor(pos.z + 0.5f));
		} else
		{
			pos = _hit.transform.gameObject.transform.position + _hit.normal;
		}
		Debug.DrawRay(_hit.point, _hit.normal*2, Color.green);
		foreach (GameObject go in f_followMouse)
		{
			go.transform.position = pos * go.transform.localScale.x;
			if (Input.GetMouseButtonDown(0))
			{
				Debug.Log(UnityEngine.UIElements.);
				GameObject newGo = UnityEngine.Object.Instantiate(prefab, pos, Quaternion.identity);
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