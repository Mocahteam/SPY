using UnityEngine;
using FYFY;

/// <summary>
/// This system enables to manage game mode: playmode vs editmode
/// </summary>
public class ModeManager : FSystem {

	public static ModeManager instance;

    public ModeManager()
	{
		instance = this;
	}
	
	public void setPlayingMode(){
		GameObjectManager.addComponent<PlayMode>(MainLoop.instance.gameObject);
		// in case of several EditMode components exist
		foreach(EditMode em in MainLoop.instance.gameObject.GetComponents<EditMode>())
			GameObjectManager.removeComponent(em);
	}
	
	public void setEditMode()
	{
		GameObjectManager.addComponent<EditMode>(MainLoop.instance.gameObject);
		// in case of several PlayMode components exist
		foreach (PlayMode pm in MainLoop.instance.gameObject.GetComponents<PlayMode>())
			GameObjectManager.removeComponent(pm);
	}
}
