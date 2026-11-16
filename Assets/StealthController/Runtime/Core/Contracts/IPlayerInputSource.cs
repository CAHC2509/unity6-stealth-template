namespace StealthController.Core
{
    public interface IPlayerInputSource
    {
        PlayerInputFrame ConsumeFrame();
    }
}
