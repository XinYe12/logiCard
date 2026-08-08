namespace LogiCard.Audio
{
    public enum FoleyId
    {
        Footstep,
        Shot,
        TimeCard,
        LockIn,
    }

    public interface IFoleyPlayer
    {
        void Play(FoleyId id);
    }
}
