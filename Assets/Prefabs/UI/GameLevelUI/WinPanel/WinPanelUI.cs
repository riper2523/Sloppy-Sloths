using UnityEngine;
using TMPro;

public class WinPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText; 

    public void DisplayResults(bool[] earnedStars)
    {
        int count = 0;

        foreach (bool earned in earnedStars)
        {
            if (earned) count++;
        }

        resultText.text = "You won with " + count + " Stars Collected!";
    }
}
