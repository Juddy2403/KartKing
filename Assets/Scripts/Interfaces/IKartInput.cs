using UnityEngine;

public interface IKartInput {
    float          Steering   { get; } // -1 .. 1
    bool           Accelerate { get; }
    bool           Brake      { get; }
    public Vector2 Aim        { get; }
    public bool    Shoot      { get; }
}