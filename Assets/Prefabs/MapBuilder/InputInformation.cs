using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // Required for IsPointerOverGameObject
using Assets.Prefabs.MapBuilder;

class InputInformation : MonoBehaviour, IInputInformation
{
    private Camera mainCam;

    private float DistanceFromTheWorldToTheCamera;

    void Start()
    {
        mainCam = Camera.main;
        Debug.Assert(mainCam is not null);
        DistanceFromTheWorldToTheCamera = -mainCam.transform.position.z;
    }

    public bool DelKeyWasClicked()
    {
        return Keyboard.current is not null && Keyboard.current.deleteKey.wasPressedThisFrame;
    }

    public Vector3 GetMouseWorldPos()
    {
        var mousePos2D = Mouse.current.position.ReadValue();
        Vector3 mousePos3D = new(mousePos2D.x, mousePos2D.y, DistanceFromTheWorldToTheCamera);

        return mainCam.ScreenToWorldPoint(mousePos3D);
    }

    public bool WeReleasedThisFrame()
    {
        return Mouse.current is not null && Mouse.current.leftButton.wasReleasedThisFrame;
    }

    public bool WeClickedThisFrame()
    {
        return Mouse.current is not null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    public bool IsCtrlPressed()
    {
        return Keyboard.current is not null && Keyboard.current.ctrlKey.isPressed;
    }

    public bool EscapeKeyWasClicked()
    {
        return Keyboard.current is not null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    public bool VoidWasClicked()
    {
        if (WeClickedThisFrame() == false)
        {
            return false;
        }

        if (EventSystem.current is null)
        {
            Debug.LogError(
                    "Event system is null");
            return false;
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        return true;
    }
}
