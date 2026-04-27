using UnityEngine;
using TMPro;

public class WinPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText; 

    public void DisplayResults(LevelResult result)
    {
        int count = 0;

        foreach (StarResult starResult in result.starResults)
        {
            if (starResult.achieved) count++;
        }

        resultText.text = "You won with " + count + " Stars Collected!\nTime: " + result.completionTime.ToString("F2") + " seconds";
    }
}
