using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ActionSprites", menuName = "Scriptable Objects/ActionSprites")]
public class ActionSprites : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    public struct ActionIconMapping
    {
        public ActionType actionType;
        public ActionIconSprites iconSprites;
    }
    [Serializable]
    public struct ActionIconSprites
    {
        public Sprite iconSpriteOn;
        public Sprite iconSpriteOff;
    }

    [Header("Konfiguracja w Inspektorze (Lista)")]
    [SerializeField] private List<ActionIconMapping> iconMappings = new List<ActionIconMapping>();

    public Dictionary<ActionType, ActionIconSprites> iconDictionary = new Dictionary<ActionType, ActionIconSprites>();
    public void OnBeforeSerialize()
    {
    }
    public void OnAfterDeserialize()
    {
        iconDictionary.Clear();

        foreach (var mapping in iconMappings)
        {
            if (!iconDictionary.ContainsKey(mapping.actionType))
            {
                iconDictionary.Add(mapping.actionType, mapping.iconSprites);
            }
        }
    }
}
