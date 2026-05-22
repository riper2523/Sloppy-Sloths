using UnityEngine;

[CreateAssetMenu(fileName = "CurrentSessionSO", menuName = "Scriptable Objects/Current Session")]
public class CurrentSessionSO : ScriptableObject
{
    [Header("Session State")]
    public LevelData activeLevel;
}
