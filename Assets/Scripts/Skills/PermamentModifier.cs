public class PermamentModifier : Modifier
{
    public PermamentModifier(float modifer, Mod mod, MyAttribute attribute) : this( modifer, mod, "" ,attribute )
    {
    }

    public PermamentModifier(float modifer, Mod mod, string modifierName, MyAttribute attribute) : base( modifer, mod, modifierName, attribute )
    {
    }
}
