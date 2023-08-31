using UnityEngine;
using FYFY;

public class ViewToggleSystem : FSystem
{
	public Canvas mainCanvas;
	public Canvas metadataCanvas;
	public PaintableGrid paintableGrid;

	private bool mainCanvasActive = true;

	public static ViewToggleSystem instance;

	public ViewToggleSystem()
	{
		instance = this;
	}
	
	// Use to init system before the first onProcess call
	protected override void onStart(){
		GameObjectManager.setGameObjectState(metadataCanvas.gameObject, false);
	}

	public void toggleCanvas()
	{
		mainCanvasActive = !mainCanvasActive;
		GameObjectManager.setGameObjectState(mainCanvas.gameObject, mainCanvasActive);
		paintableGrid.gridActive = mainCanvasActive;
		GameObjectManager.setGameObjectState(paintableGrid.gameObject, mainCanvasActive);
		GameObjectManager.setGameObjectState(metadataCanvas.gameObject, !mainCanvasActive);
	}
}