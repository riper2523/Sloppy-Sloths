using UnityEngine;

[CreateAssetMenu(fileName = "CurrentSessionSO", menuName = "Scriptable Objects/Current Session")]
public class CurrentSessionSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("Session State")]
    public LevelData activeLevel;
    public ChapterData activeChapter;
    public bool returnToLevelSelection;
    public void OnAfterDeserialize()
    {
        returnToLevelSelection = false;
    }
    public void OnBeforeSerialize()
    {
        if (!Application.isPlaying)
        {
            returnToLevelSelection = false;
        }
    }
}
