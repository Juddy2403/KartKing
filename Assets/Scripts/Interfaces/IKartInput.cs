using UnityEngine;

namespace Interfaces {
    public interface IKartInput {
        Vector2        Steering   { get; } // -1 .. 1
        public Vector2 Aim        { get; }
        public bool    ShootStarted      { get; }
        public bool    ShootPerformed      { get; }
    }
}