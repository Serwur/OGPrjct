using ColdCry.Utility.Patterns.Builder;
using System;
/// <summary>
/// <br>Klasa przeznaczona do sprawnego modyfikowania statystyk postaci/przeciwników. Używa się jej</br>
/// <br>klas bazowych, którymi są <code>PermamentModifier</code> oraz <code>TemplateModifier</code>.</br>
/// <br>Jak same nazwy mówią pierwszy modifier działa przez cały okres czasu życia jednostki,</br>
/// <br>natomiast drugi znika po określonym czasie.</br>
/// </summary>
namespace ColdCry
{
    public class Modifier : ICloneable
    {
        public Modifier(float value, string name, ModifierEffect effect)
        {
            Value = value;
            Name = name;
            Effect = effect;
        }

        public float GetModifier()
        {
            if (Effect == ModifierEffect.POSITIVE) {
                return Value + 1;
            }
            throw new NotImplementedException();
        }

        public virtual object Clone()
        {
            return new Modifier( Value, Name, Effect );
        }

        public float Value { get; private set; }
        public string Name { get; private set; }
        public ModifierEffect Effect { get; private set; }
    }

    public class Builder : IBuilder<Modifier>
    {
        private float value = 0f;
        private string name = "";
        private ModifierEffect effect = ModifierEffect.POSITIVE;
        private Attribute attribute;

        private Builder()
        { }

        public Builder Value(float value)
        {
            this.value = value;
            return this;
        }

        public Builder Name(string name)
        {
            this.name = name;
            return this;
        }

        public Builder Effect(ModifierEffect effect)
        {
            this.effect = effect;
            return this;
        }

        public Builder Positive()
        {
            effect = ModifierEffect.POSITIVE;
            return this;
        }

        public Builder Negative()
        {
            effect = ModifierEffect.NEGATIVE;
            return this;
        }

        public Builder Clone(Modifier modifier)
        {
            value = modifier.Value;
            name = modifier.Name;
            effect = modifier.Effect;
            return this;
        }

        public Builder Attribute(Attribute attribute)
        {
            this.attribute = attribute;
            return this;
        }

        public Modifier Build()
        {
            if (attribute == null) {
                throw new ArgumentNullException( "Attribute cannot be null" );
            }
            Modifier modifier = new Modifier( value, name, effect );
            attribute.AddModifier( modifier );
            return modifier;
        }

        public static Builder Get { get => new Builder(); }
    }



}