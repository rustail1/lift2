public interface IInteractable
{
    string GetInteractionText();
    bool CanInteract();
    void InteractDown();
    void InteractUp();
    void InteractionTick(float deltaTime);
    float GetInteractionProgress();
    InteractionType GetInteractionType();
    bool IsInteractionInProgress { get; }
}
