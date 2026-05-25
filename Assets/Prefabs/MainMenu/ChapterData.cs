using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChapter", menuName = "Scriptable Objects/Chapter Data")]
public class ChapterData : ScriptableObject
{
    public string chapterName;
    public Sprite icon;
    public List<LevelData> levels;
}
