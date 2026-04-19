using UnityEngine;
using UnityEngine.InputSystem;
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
}
