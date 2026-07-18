namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// Produces InputCommands from some source (touch, keyboard/mouse, AI, network).
    /// The simulation only ever talks to this interface, never to the source directly.
    /// </summary>
    public interface IInputDriver
    {
        InputCommand Sample(double time);
    }
}
