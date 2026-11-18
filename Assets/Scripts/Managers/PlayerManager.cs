using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// Persistent singleton. Handles joining/leaving with a button press, device
/// disconnects/reconnects, and bot slots. Lives across Menu -> Arena scenes.
/// </summary>
public class PlayerManager : MonoBehaviour {
    public const int MaxPlayers        = 12;
    public const int MinPlayersToStart = 2;

    public static PlayerManager Instance { get; private set; }

    [SerializeField] private InputActionAsset kartControls;
    [SerializeField] private Color[]          playerColors;

    private readonly PlayerSlot[] slots = new PlayerSlot[MaxPlayers];
    private          IDisposable  anyButtonListener;

    /// <summary>True in the lobby (join/leave allowed). Set false when a match starts.</summary>
    public bool AcceptingJoins { get; set; } = true;

    public IEnumerable<PlayerSlot> Players     => slots.Where(s => s != null);
    public int                     PlayerCount => Players.Count();
    public bool                    CanStart    => PlayerCount >= MinPlayersToStart;

    public event Action<PlayerSlot> PlayerJoined;
    public event Action<PlayerSlot> PlayerLeft;
    public event Action<PlayerSlot> PlayerStateChanged; // disconnected / reconnected / reclaimed

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() {
        InputSystem.onDeviceChange += OnDeviceChange;
        anyButtonListener          =  InputSystem.onAnyButtonPress.Call(OnAnyButtonPressed);
    }

    private void OnDisable() {
        InputSystem.onDeviceChange -= OnDeviceChange;
        anyButtonListener?.Dispose();
    }

    private void OnAnyButtonPressed(InputControl control) {
        InputDevice device = control.device;

        bool confirm = false, cancel = false;
        if (device is Gamepad pad) {
            confirm = control == pad.buttonSouth;
            cancel  = control == pad.buttonEast;
        }
        else if (device is Keyboard kb) {
            confirm = control == kb.spaceKey;
            cancel  = control == kb.escapeKey;
        }

        if (confirm) HandleConfirm(device);
        else if (cancel) HandleCancel(device);
    }

    private void HandleConfirm(InputDevice device) {
        if (FindSlot(device) != null) return; // already a player
        if (AcceptingJoins) TryJoin(device);
        else TryReclaim(device); // mid-match: take over a slot that lost its pad
    }

    private void HandleCancel(InputDevice device) {
        if (!AcceptingJoins) return;
        PlayerSlot slot = FindSlot(device);
        if (slot != null) Leave(slot);
    }

    public PlayerSlot TryJoin(InputDevice device) {
        int index = FirstFreeIndex();
        if (index < 0) return null;

        var slot = new PlayerSlot(index, ColorFor(index), kartControls, device);
        slots[index] = slot;
        PlayerJoined?.Invoke(slot);
        return slot;
    }

    public void Leave(PlayerSlot slot) {
        if (slot == null || slots[slot.Index] != slot) return;
        slots[slot.Index] = null;
        slot.Dispose();
        PlayerLeft?.Invoke(slot);
    }

    public PlayerSlot AddBot() {
        int index = FirstFreeIndex();
        if (index < 0) return null;

        var slot = new PlayerSlot(index, ColorFor(index), kartControls, null);
        slots[index] = slot;
        PlayerJoined?.Invoke(slot);
        return slot;
    }

    public void RemoveLastBot() {
        PlayerSlot bot = Players.LastOrDefault(s => s.IsBot);
        if (bot != null) Leave(bot);
    }

    /// <summary>Call when returning to the main menu.</summary>
    public void ClearAll() {
        foreach (PlayerSlot slot in Players.ToList()) Leave(slot);
        AcceptingJoins = true;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        PlayerSlot slot = FindSlot(device);
        if (slot == null) return;

        switch (change) {
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Removed:
                if (AcceptingJoins) Leave(slot); // lobby: just drop the seat
                else {
                    slot.SetConnected(false); // match: keep the seat, kart coasts
                    PlayerStateChanged?.Invoke(slot);
                }
                break;
            case InputDeviceChange.Reconnected:
                slot.SetConnected(true);
                PlayerStateChanged?.Invoke(slot);
                break;
        }
    }

    private void TryReclaim(InputDevice device) {
        PlayerSlot lost = Players.FirstOrDefault(slot => !slot.IsBot && !slot.IsConnected);
        if (lost == null) return;

        lost.AssignDevice(device);
        PlayerStateChanged?.Invoke(lost);
    }

#region Helpers
    private PlayerSlot FindSlot(InputDevice device) =>
        slots.FirstOrDefault(s => s != null && s.Device == device);

    private int FirstFreeIndex() {
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] == null)
                return i;
        return -1;
    }

    private Color ColorFor(int index) =>
        playerColors != null && playerColors.Length > 0
            ? playerColors[index % playerColors.Length]
            : Color.HSVToRGB(index / (float)MaxPlayers, 0.8f, 1f);

#endregion
}