using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Prefabs.MapBuilder.Node
{
  class NodeController : MonoBehaviour, IPointerClickHandler
  {
    private Vector3 lastPosition;
    [SerializeField] private float epsilon = 0.0001f;

    private Collider2D nodeCollider;
    public event EventHandler? NodeActivated;
    public event EventHandler? NodeDeleted;
    public event EventHandler? NodeMoved;

    void Start()
    {
      nodeCollider = GetComponent<Collider2D>();
      lastPosition = transform.position;
    }

    public bool doesCollide(Vector2 point)
    {
      Collider2D hitCollider = Physics2D.OverlapPoint(point);

      if (hitCollider != null && hitCollider == nodeCollider)
      {
        return true;
      }
      return false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      Debug.Log("Node activated");
      NodeActivated?.Invoke(this, EventArgs.Empty);
    }

    public void deleteNode()
    {
      Debug.Log("Object deleted");
      NodeDeleted?.Invoke(this, EventArgs.Empty);
      Destroy(gameObject);
    }

    public Vector2 getCoordinates()
    {
      return transform.position;
    }

    public void Update()
    {
      if ((transform.position -
lastPosition).sqrMagnitude > epsilon *
epsilon)
      {
        lastPosition = transform.position;
        Debug.Log("Node moved");
        NodeMoved?.Invoke(this, EventArgs.Empty);
      }
    }
  }
}

