namespace ColdCry
{
    public class PermamentModifier : Modifier
    {
        public PermamentModifier(float modifer, Mod mod, Attribute attribute) : this( modifer, mod, "", attribute )
        {
        }

        public PermamentModifier(float modifer, Mod mod, string modifierName, Attribute attribute) : base( modifer, mod, modifierName, attribute )
        {
        }
    }
}