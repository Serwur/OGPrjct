using ColdCry.Objects;

namespace ColdCry.Notifers
{
    public interface IHitPointsObservable
    {
        void AddHitPointsObserver(IHitPointsObserver observer);
        void RemoveHitPointsObserver(IHitPointsObserver observer);
    }
}
