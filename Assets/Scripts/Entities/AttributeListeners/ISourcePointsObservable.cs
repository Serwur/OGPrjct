namespace ColdCry.Notifers
{
    public interface ISourcePointsObservable
    {
        void AddSourcePointsObserver(ISourcePointsObserver observer);
        void RemoveSourcePointsObserver(ISourcePointsObserver observer);
    }
}
