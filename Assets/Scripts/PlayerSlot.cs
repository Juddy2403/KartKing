using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// One seat in the match. Owns its own copy of the input actions, restricted to a single device,
/// so several players can use the same bindings without reading each other's input.
/// A slot with no device is a bot.
/// </summary>
public class PlayerSlot
{
    public int Index { get; }
    public Color Color { get; }
    public bool IsBot { get; }
    public InputDevice Device { get; private set; }
    public InputActionAsset Actions { get; private set; }
    public bool IsConnected { get; private set; }

    public string DisplayName => IsBot ? $"Bot {Index + 1}" : $"Player {Index + 1}";

    public PlayerSlot(int index, Color color, InputActionAsset template, InputDevice device)
    {
        Index = index;
        Color = color;
        IsBot = device == null;

        if (IsBot)
        {
            IsConnected = true; // bots are never "lost"
            return;
        }

        Actions = Object.Instantiate(template); // private copy of the action asset
        AssignDevice(device);
    }

    public void AssignDevice(InputDevice device)
    {
        Device = device;
        IsConnected = true;
        Actions.devices = new[] { device }; // this copy only listens to this device
    }

    public void SetConnected(bool connected) => IsConnected = connected;

    public void Dispose()
    {
        if (Actions == null) return;
        Actions.Disable();
        Object.Destroy(Actions);
        Actions = null;
    }
}
