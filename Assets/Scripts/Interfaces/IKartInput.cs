/// <summary>
/// What a kart wants to do this frame. Humans and bots both implement this,
/// so PlayerMovement never needs to know who is driving.
/// </summary>
public interface IKartInput
{
    float Steering { get; }   // -1 .. 1
    bool Accelerate { get; }
    bool Brake { get; }
}
