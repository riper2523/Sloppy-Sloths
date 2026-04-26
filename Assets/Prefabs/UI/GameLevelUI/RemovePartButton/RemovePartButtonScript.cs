using UnityEngine;
using UnityEngine.UI;

public class RemovePartButtonScript : MonoBehaviour
{
    [SerializeField]
    private Image buttonImage;
    [SerializeField]
    private Button button;
    [SerializeField]
    private GridManager gridManager;
    [SerializeField]
    private Sprite sprite;
    void Start()
    {
        if (gridManager != null)
        {
            Setup();
        }

    }
    public void Initialize(GridManager manager)
    {
        gridManager = manager;
        Setup();
    }
    private void Setup()
    {
        button.onClick.AddListener(() =>
                   {
                       gridManager.SelectPart(null);
                   });
        buttonImage.sprite = sprite;
    }
}
