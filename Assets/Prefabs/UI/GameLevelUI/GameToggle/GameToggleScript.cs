using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static ActionSprites;

public class GameToggleScript : MonoBehaviour
{
    [SerializeField]
    private Image toggleImage;
    private ActionIconSprites actionIconSprites;
    public UnityEvent startEvent;
    public UnityEvent endEvent;

    public void Toggle(Boolean isOn)
    {
        if (isOn)
        {
            toggleImage.sprite = actionIconSprites.iconSpriteOn;
            startEvent.Invoke();
        }
        else
        {
            toggleImage.sprite = actionIconSprites.iconSpriteOff;
            endEvent.Invoke();
        }
    }
    public void Initialize(ActionIconSprites actionIconSprites)
    {
        this.actionIconSprites = actionIconSprites;
        toggleImage.sprite = actionIconSprites.iconSpriteOff;
    }
}
