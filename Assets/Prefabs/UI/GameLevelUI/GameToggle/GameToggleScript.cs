using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameToggleScript : MonoBehaviour
{
    [SerializeField]
    private Image toggleImage;
    public UnityEvent startEvent;
    public UnityEvent endEvent;

    public void Toggle(Boolean isOn)
    {
        if (isOn)
        {
            startEvent.Invoke();
        }
        else
        {
            endEvent.Invoke();
        }
    }
    public void Initialize(Sprite image)
    {
        toggleImage.sprite = image;
    }
}
