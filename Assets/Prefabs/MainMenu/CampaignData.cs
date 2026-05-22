using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MainCampaign", menuName = "Scriptable Objects/Campaign Data")]
public class CampaignData : ScriptableObject
{
    public List<ChapterData> chapters;
}
