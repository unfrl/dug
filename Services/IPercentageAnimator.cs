namespace dug.Services
{
    public interface IPercentageAnimator
    {
        void EventHandler(string customString);

        void Start(string header, double totalEvents, int progressBarLength = 50);

        void StopIfRunning();
    }
}