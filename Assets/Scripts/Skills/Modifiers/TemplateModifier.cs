using ColdCry.Utility.Time;

namespace ColdCry
{
    public class TemplateModifier : Modifier
    {
        public ICountdown countdown;
        public float time = 0;

        protected TemplateModifier(float value, string name, ModifierEffect effect) : this( value, name, effect, 10f )
        {
        }

        protected TemplateModifier(float value, string name, ModifierEffect effect, float time) : base( value, name, effect )
        {
            this.time = time;
            countdown = TimerManager.Start( time, (overtime) => {
                countdown.Destroy();
                countdown = null;
            } );
        }



        /*
        public override object Clone(Attribute attribute)
        {
            TemplateModifier clone = (TemplateModifier) base.Clone( attribute );
            clone.time = time;
            clone.countdown = countdown.Clone() as ICountdown;
            return clone;
        }*/
    }
}