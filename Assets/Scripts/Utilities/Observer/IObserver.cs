namespace ColdCry.Utility
{
    public interface IObserver<T>
    {
        void Notify(T notifier);
    }
}
