using System;

public static class GameEvents
{
    // Event to notify when the gravity switch state changes
    public static event Action<bool> OnGravitySwitchToggled;

    public static void TriggerGravitySwitchToggled(bool isActivated)
    {
        OnGravitySwitchToggled?.Invoke(isActivated);
    }
}
