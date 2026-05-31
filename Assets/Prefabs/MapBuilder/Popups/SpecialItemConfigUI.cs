using UnityEngine;
using TMPro;
using Assets.Prefabs.MapBuilder.Node.SpecialItems;

namespace Assets.Prefabs.MapBuilder.Popups
{
    public class SpecialItemConfigUI : MonoBehaviour
    {
        [SerializeField] protected UnityEngine.UI.Button deleteButton;
        protected ISpecialItemController CurrentSpecialItem;

        protected virtual void Start()
        {
            if (deleteButton == null)
            {
                var buttons = GetComponentsInChildren<UnityEngine.UI.Button>(true);
                foreach (var b in buttons)
                {
                    if (b.gameObject.name == "DeleteButton")
                    {
                        deleteButton = b;
                        break;
                    }
                }
            }

            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(OnDeleteClicked);
            }
        }

        protected virtual void OnDeleteClicked()
        {
            if (CurrentSpecialItem != null)
            {
                CurrentSpecialItem.Delete();
                Trigger(null); // Hide UI after deletion
            }
        }

        public virtual void Trigger(ISpecialItemController specialItem)
        {
            CurrentSpecialItem = specialItem;
            gameObject.SetActive(specialItem is not null);
        }
    }
}
