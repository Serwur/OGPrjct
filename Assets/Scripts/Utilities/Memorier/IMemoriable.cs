namespace ColdCry.Utility.Patterns.Memory
{
    public interface IMemoriable
    {
        MemoryPart SaveMemory();
        void LoadMemory(MemoryPart data);
    }
}
