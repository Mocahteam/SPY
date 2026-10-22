using UnityEngine;

public class TilePopupSystemBridge : MonoBehaviour
{
	public void RemoveTileSettings()
    {
        TilePopupSystem.instance.RemoveTileSettings(gameObject);
    }

    public void MoveTile(string position)
    {
        TilePopupSystem.instance.MoveTile(gameObject, position);
    }

    public void RotateObject(int newOrientation)
    {
        TilePopupSystem.instance.RotateObject(gameObject, newOrientation);
    }

    public void PopUpInputLine(string newData)
    {
        TilePopupSystem.instance.PopUpInputLine(gameObject, newData);
    }

    public void PopupRangeInputField(string newData)
    {
        TilePopupSystem.instance.PopupRangeInputField(gameObject, newData);
    }

    public void PopupRangeToggle(bool newData)
    {
        TilePopupSystem.instance.PopupRangeToggle(gameObject, newData);
    }

    public void PopupRangeDropDown(int newData)
    {
        TilePopupSystem.instance.PopupRangeDropDown(gameObject, newData);
    }

    public void PopupConsoleSlots(string newData)
    {
        TilePopupSystem.instance.PopupConsoleSlots(gameObject, newData);
    }

    public void PopupDoorSlot(string newData)
    {
        TilePopupSystem.instance.PopupDoorSlot(gameObject, newData);
    }

    public void PopupDoorToggle(bool newData)
    {
        TilePopupSystem.instance.PopupDoorToggle(gameObject, newData);
    }

    public void PopupFurnitureDropDown(int newData)
    {
        TilePopupSystem.instance.PopupFurnitureDropDown(gameObject, newData);
    }

    public void PopupSkinDropDown(int newData)
    {
        TilePopupSystem.instance.PopupSkinDropDown(gameObject, newData);
    }
}
