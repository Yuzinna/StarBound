using UnityEngine;

public class Cube : MonoBehaviour, IInteractable
{
    public bool canBeMoved { get; private set; } = false;

    private void OnEnable()
    {
        GameEvents.OnGravitySwitchToggled += OnGravitySwitchToggled;
    }

    private void OnDisable()
    {
        GameEvents.OnGravitySwitchToggled -= OnGravitySwitchToggled;
    }

    private void OnGravitySwitchToggled(bool isActivated)
    {
        canBeMoved = isActivated;
    }

    public void Interact()
    {
        // The actual push/pull logic will be handled by the PlayerMovement script.
        // This Interact method is called to signify that the player is
        // initiating an interaction with this cube.
        // No specific action needed here, as PlayerMovement will handle the joint creation.
    }
}
