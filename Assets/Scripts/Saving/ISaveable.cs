namespace RPG.Saving
{
    // Implemented to save a particular game state
    public interface ISaveable
    {
        object CaptureState();

        void RestoreState(object state);
    }
}