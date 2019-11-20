using ColdCry.Objects;

namespace ColdCry.Notifers
{
    public interface IHitPointsObserver 
    {
        void NotifyHitPointsChange(Entity entity, Attribute attribute, float change);
    }
}
