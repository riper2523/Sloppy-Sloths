using UnityEngine;
using TMPro;

public class WinPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText; 

    public void DisplayResults()
    {
        resultText.text = "You won!";
    }
}
