using UnityEngine;

public class GravitySwitch : MonoBehaviour, IInteractable
{
    [Tooltip("The gravity to apply when the switch is activated.")]
    public Vector2 gravityDirection = new Vector2(0, -9.81f);

    private bool isActivated = false;
    private Vector2 originalGravity;

    void Start()
    {
        originalGravity = Physics2D.gravity;
    }

    /// <summary>
    /// Toggles the switch's state, changes the global gravity,
    /// and notifies other objects of the state change via the GameEvents system.
    /// </summary>
    public void Interact()
    {
        isActivated = !isActivated;

        if (isActivated)
        {
            Physics2D.gravity = gravityDirection;
        }
        else
        {
            Physics2D.gravity = originalGravity;
        }

        // Trigger the event to inform other objects (like cubes) about the state change.
        GameEvents.TriggerGravitySwitchToggled(isActivated);
    }
}
