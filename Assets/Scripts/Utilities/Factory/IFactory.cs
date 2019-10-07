namespace ColdCry.Utility.Patterns.Factory
{
    public interface IFactory<T>
    {
        T GetInstance<R>();
    }
}
