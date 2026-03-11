using UnityEngine;

public class TilePopupSystemBridge : MonoBehaviour
{
	public void removeTileSettings()
    {
        TilePopupSystem.instance.removeTileSettings(gameObject);
    }

    public void moveTile(string position)
    {
        TilePopupSystem.instance.moveTile(gameObject, position);
    }

    public void rotateObject(int newOrientation)
    {
        TilePopupSystem.instance.rotateObject(gameObject, newOrientation);
    }

    public void popUpInputLine(string newData)
    {
        TilePopupSystem.instance.popUpInputLine(gameObject, newData);
    }

    public void popupRangeInputField(string newData)
    {
        TilePopupSystem.instance.popupRangeInputField(gameObject, newData);
    }

    public void popupRangeToggle(bool newData)
    {
        TilePopupSystem.instance.popupRangeToggle(gameObject, newData);
    }

    public void popupRangeDropDown(int newData)
    {
        TilePopupSystem.instance.popupRangeDropDown(gameObject, newData);
    }

    public void popupConsoleSlots(string newData)
    {
        TilePopupSystem.instance.popupConsoleSlots(gameObject, newData);
    }

    public void popupDoorSlot(string newData)
    {
        TilePopupSystem.instance.popupDoorSlot(gameObject, newData);
    }

    public void popupDoorToggle(bool newData)
    {
        TilePopupSystem.instance.popupDoorToggle(gameObject, newData);
    }

    public void popupFurnitureDropDown(int newData)
    {
        TilePopupSystem.instance.popupFurnitureDropDown(gameObject, newData);
    }

    public void popupSkinDropDown(int newData)
    {
        TilePopupSystem.instance.popupSkinDropDown(gameObject, newData);
    }
}
