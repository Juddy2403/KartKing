using System.Collections.Generic;
using Managers;
using UnityEngine;

public class KartSpawner : MonoBehaviour
{
    [SerializeField] private GameObject      kartPrefab;
    [SerializeField] private List<Transform> spawnPoints;

    private void Start()
    {
        PlayerManager manager = PlayerManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("No PlayerManager. Start from the main menu scene.");
            return;
        }

        spawnPoints.Shuffle();
        
        foreach (PlayerSlot slot in manager.Players)
        {
            Transform point = spawnPoints[slot.Index % spawnPoints.Count];
            GameObject go = Instantiate(kartPrefab, point.position, point.rotation);
            go.name = slot.DisplayName;
            go.GetComponent<Kart>().Initialize(slot);
        }
    }
}
