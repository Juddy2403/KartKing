using UnityEngine;

[RequireComponent(typeof(ShootingController))]
public class Kart : MonoBehaviour
{
    public  PlayerSlot         Slot { get; private set; }
    private ShootingController shootingController;

    private void Awake() {
        shootingController = GetComponent<ShootingController>();
    }
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

    private void OnTriggerEnter(Collider other) {
        if (!other.gameObject.CompareTag("Ball")) return;
        shootingController.enabled = true;
        Destroy(other.gameObject); 
    }
    
}
