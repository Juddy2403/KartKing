using UnityEngine;

namespace Spawners {
    public class CrownSpawner : MonoBehaviour {
        [SerializeField] private BoxCollider spawnBounds;
        [SerializeField] private GameObject  crownPrefab;
        [SerializeField] private GameObject  spawnVFX;
        [SerializeField] private Vector3     spawnOffset;

        private void Awake() {
            if (!Helpers.TryGetRandomPoint(spawnBounds.bounds, out Vector3 pos)) {
                Debug.LogError($"Could not find Spawn Bounds at {spawnBounds}");
            }

            Instantiate(crownPrefab, pos + spawnOffset, Quaternion.identity);
            Instantiate(spawnVFX, pos + spawnOffset, Quaternion.identity);
        }
    }
}