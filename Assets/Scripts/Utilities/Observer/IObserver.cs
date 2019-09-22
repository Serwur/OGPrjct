namespace ColdCry.Utility
{
    interface IObserver<T>
    {
       void Notify(T notifier);
    }
}
