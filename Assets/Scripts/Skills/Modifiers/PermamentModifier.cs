public class PermamentModifier : Modifier
{
    public PermamentModifier(float modifer, Mod mod, PAttribute attribute) : this( modifer, mod, "" ,attribute )
    {
    }

    public PermamentModifier(float modifer, Mod mod, string modifierName, PAttribute attribute) : base( modifer, mod, modifierName, attribute )
    {
    }
}
