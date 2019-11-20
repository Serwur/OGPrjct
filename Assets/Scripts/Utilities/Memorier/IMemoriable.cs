namespace ColdCry.Utility.Patterns.Memory
{
    public interface IMemoriable<T>
    {
        T SaveMemory();
        void LoadMemory(T data);
    }
}
