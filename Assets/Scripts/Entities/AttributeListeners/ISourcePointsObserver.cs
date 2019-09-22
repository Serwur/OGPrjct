using ColdCry.Objects;

namespace ColdCry.Notifers
{
    public interface ISourcePointsObserver
    {
        void NotifySourcePointsChange(Entity entity, Attribute attribute, float change);
    }
}
