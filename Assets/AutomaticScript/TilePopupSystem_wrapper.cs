using UnityEngine;
using FYFY;

public class TilePopupSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject orientationPopup;
	public UnityEngine.GameObject inputLinePopup;
	public UnityEngine.GameObject rangePopup;
	public UnityEngine.GameObject consoleSlotsPopup;
	public UnityEngine.GameObject doorSlotPopup;
	public UnityEngine.GameObject furniturePopup;
	public PaintableGrid paintableGrid;
	public UnityEngine.GameObject selection;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "orientationPopup", orientationPopup);
		MainLoop.initAppropriateSystemField (system, "inputLinePopup", inputLinePopup);
		MainLoop.initAppropriateSystemField (system, "rangePopup", rangePopup);
		MainLoop.initAppropriateSystemField (system, "consoleSlotsPopup", consoleSlotsPopup);
		MainLoop.initAppropriateSystemField (system, "doorSlotPopup", doorSlotPopup);
		MainLoop.initAppropriateSystemField (system, "furniturePopup", furniturePopup);
		MainLoop.initAppropriateSystemField (system, "paintableGrid", paintableGrid);
		MainLoop.initAppropriateSystemField (system, "selection", selection);
	}

	public void rotateObject(System.Int32 newOrientation)
	{
		MainLoop.callAppropriateSystemMethod (system, "rotateObject", newOrientation);
	}

	public void popUpInputLine(System.String newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "popUpInputLine", newData);
	}

	public void popupRangeInputField(System.String newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "popupRangeInputField", newData);
	}

	public void popupRangeToggle(System.Boolean newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "popupRangeToggle", newData);
	}

	public void popupRangeDropDown(System.Int32 newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "popupRangeDropDown", newData);
	}

	public void popupConsoleSlots(System.String newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "popupConsoleSlots", newData);
	}

	public void popupConsoleToggle(System.Boolean newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "popupConsoleToggle", newData);
	}

	public void popupDoorSlot(System.String newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "popupDoorSlot", newData);
	}

	public void popupFurnitureDropDown(System.Int32 newData)
	{
		MainLoop.callAppropriateSystemMethod (system, "popupFurnitureDropDown", newData);
	}

}
