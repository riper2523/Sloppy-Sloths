using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler
{
  private Camera mainCamera;
  private float zDistance;
  private Vector3 offset;

  void Awake()
  {
    mainCamera = Camera.main;
  }

  void Start()
  {
    mainCamera = Camera.main;
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    zDistance = mainCamera.WorldToScreenPoint(transform.position).z;
    offset = transform.position - GetMouseWorldPos();
  }

  public void OnDrag(PointerEventData eventData)
  {
    transform.position = GetMouseWorldPos() + offset;
  }

  private Vector3 GetMouseWorldPos()
  {
    Vector2 mousePos2D = Mouse.current.position.ReadValue();

    Vector3 mousePos3D = new(mousePos2D.x, mousePos2D.y, zDistance);
    return mainCamera.ScreenToWorldPoint(mousePos3D);
  }
}
