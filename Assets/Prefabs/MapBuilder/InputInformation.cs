using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // Required for IsPointerOverGameObject
using Assets.Prefabs.MapBuilder;

class InputInformation : MonoBehaviour, IInputInformation
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    public bool DelKeyWasClicked()
    {
        return Keyboard.current != null && Keyboard.current.deleteKey.wasPressedThisFrame;
    }

    public Vector3 GetMouseWorldPos()
    {
        Vector2 mousePos2D = Mouse.current.position.ReadValue();

        Vector3 mousePos3D = new(mousePos2D.x, mousePos2D.y, 0);
        return mainCam.ScreenToWorldPoint(mousePos3D);
    }

    public bool WeClickedThisFrame()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    public bool IsCtrlPressed()
    {
        return Keyboard.current != null && Keyboard.current.ctrlKey.isPressed;
    }

    public bool EscapeKeyWasClicked()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.isPressed;
    }

    public bool VoidWasClicked()
    {
        if (WeClickedThisFrame() == false)
        {
            return false;
        }

        if (EventSystem.current == null)
        {
            Debug.LogError(
                    "Event system is null");
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        return true;
    }
}
