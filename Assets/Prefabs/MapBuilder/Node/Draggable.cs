#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Assets.Prefabs.MapBuilder;


public interface IDraggable
{
    event NodeDraggedHandler? nodeDragged;
    event Action? NodeDragEnded;
    bool enabled { get; set; }
}

// enabled is implemented in GameObject
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDraggable
{
    private Camera? mainCamera;
    private float zDistance;
    private Vector3 offset;

    // How much the node has to move to consider, that it actually moved
    // [SerializeField] private float deltaMoved = 0.0001f;

    public event NodeDraggedHandler? nodeDragged;
    public event Action? NodeDragEnded;

    void Awake()
    {
        mainCamera = Camera.main!;
        Debug.Assert(mainCamera is not null);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        zDistance = mainCamera!.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPos();
    }

    public void OnDrag(PointerEventData eventData)
    {
        var actPos = GetMouseWorldPos();
        Debug.Log($"Moving. Old pos: {actPos}, offset: {offset}, transform.position: {transform.position}");
        var oldTransform = transform.position;
        transform.position = actPos + offset;
        nodeDragged?.Invoke(oldTransform, transform.position - oldTransform);
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector2 mousePos2D = Mouse.current.position.ReadValue();

        Vector3 mousePos3D = new(mousePos2D.x, mousePos2D.y, zDistance);
        return mainCamera!.ScreenToWorldPoint(mousePos3D);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        NodeDragEnded?.Invoke();
    }
}
