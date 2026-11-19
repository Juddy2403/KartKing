using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Helpers {
    public static bool TryGetRandomPoint(Bounds area, out Vector3 point, int attempts = 30) {
        for (int i = 0; i < attempts; i++) {
            var candidate = new Vector3(Random.Range(area.min.x, area.max.x), area.center.y,
                Random.Range(area.min.z, area.max.z));

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
                point = hit.position;
                return true;
            }
        }
        point = default;
        return false;
    }
}

public static class ListExtensions {
    public static void Shuffle<T>(this List<T> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}