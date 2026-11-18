using UnityEngine;

/// <summary>Ties a kart GameObject to a PlayerSlot. Later: crown, ball, score, etc.</summary>
public class Kart : MonoBehaviour
{
    public PlayerSlot Slot { get; private set; }

    public void Initialize(PlayerSlot slot)
    {
        Slot = slot;

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) {
            renderer.material.color = slot.Color;
        }

        if (slot.IsBot)
        {
            // gameObject.AddComponent<BotKartInput>();   // your bot implements IKartInput
            Destroy(GetComponent<HumanKartInput>());
        }
        else
        {
            GetComponent<HumanKartInput>().Bind(slot);
        }
    }
}
