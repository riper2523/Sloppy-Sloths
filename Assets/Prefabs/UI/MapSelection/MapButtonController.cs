using UnityEngine;
using UnityEngine.UI;

public class MapButtonController : MonoBehaviour
{
    [SerializeField]
    public Button InteractionButton;

    [SerializeField]
    public Button TrashBinButton;

    void Awake()
    {
        Debug.Assert(InteractionButton is not null);
        Debug.Assert(TrashBinButton is not null);
    }
}
