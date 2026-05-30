using UnityEngine;

[DisallowMultipleComponent]
public class PickupItem : InteractableObject
{
    [Header("Pickup")]
    [Tooltip("ID предмета, который попадёт в псевдоинвентарь. Например: Screwdriver, Flashlight, Fuse, Manual, Key.")]
    [SerializeField] private string _itemId = "Screwdriver";

    [Tooltip("Событие, которое сработает после подбора. Например: ScrewdriverTaken.")]
    [SerializeField] private string _pickupEvent;

    [Tooltip("Событие, без которого предмет пока нельзя брать. Например: BagOpened.")]
    [SerializeField] private string _requiredEventToTake;

    [Tooltip("Текст, если предмет пока нельзя брать.")]
    [SerializeField] private string _cannotTakeText = "Пока не надо.";

    [Header("Pickup Options")]
    [SerializeField] private bool _disableAfterPickup = true;

    public override string GetInteractionText()
    {
        if (CanInteract())
            return base.GetInteractionText();

        return _cannotTakeText;
    }

    public override bool CanInteract()
    {
        if (!base.CanInteract())
            return false;

        if (!string.IsNullOrWhiteSpace(_requiredEventToTake))
        {
            ScenarioStateService scenario = ScenarioStateService.Instance;
            if (scenario == null || !scenario.HasEvent(_requiredEventToTake))
                return false;
        }

        return true;
    }

    public override void InteractDown()
    {
        if (!CanInteract())
        {
            InteractionUIService.Instance?.ShowHint(_cannotTakeText);
            return;
        }

        if (InventoryService.Instance != null)
            InventoryService.Instance.AddItem(_itemId);

        if (!string.IsNullOrWhiteSpace(_pickupEvent))
        {
            if (ScenarioStateService.Instance != null)
                ScenarioStateService.Instance.TriggerEvent(_pickupEvent);
            else
                GameEventBus.Trigger(_pickupEvent);
        }

        if (_disableAfterPickup)
            gameObject.SetActive(false);
    }

    public override void InteractUp()
    {
        // PickupItem срабатывает по клику. Отпускание кнопки не нужно.
    }
}
