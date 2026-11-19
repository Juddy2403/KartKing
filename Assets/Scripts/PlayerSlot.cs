using Managers;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlot
{
    public int   Index    { get; }
    public Color Color    { get; }
    public bool  IsBot    { get; }
    
    public InputDevice Device { get; private set; }
    public InputActionAsset Actions { get; private set; }
    public bool IsConnected { get; private set; }

    public string DisplayName => IsBot ? $"Bot {PlayerManager.Instance.GetColorName(Color)}" : 
        $"{PlayerManager.Instance.GetColorName(Color)}";

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
