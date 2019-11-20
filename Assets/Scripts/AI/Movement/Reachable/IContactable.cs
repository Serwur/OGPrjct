namespace ColdCry.AI
{
    public interface IContactable
    {
        void OnContactAreaEnter(ContactArea contactArea);
        void OnContactAreaExit(ContactArea contactArea);
    }
}
