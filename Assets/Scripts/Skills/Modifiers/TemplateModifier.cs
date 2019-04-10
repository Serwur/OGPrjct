using UnityEngine;

namespace DoubleMMPrjc
{
    public class TemplateModifier : Modifier, TimerManager.IOnCountdownEnd
    {
        public long countdown;
        public float time = 0;

        /// <summary>
        /// <br>Dodaje czasowy modyfikator do atrybutu. Jeżeli podany czas będzie 0 lub mniejszy, zostanie rzucony wyjątek.</br>
        /// <br>Aby rozpocząć odliczanie należy wywołać metodę StartTimer(). Wielkość modyfikacji dla paremtru Mod.POSITIVE </br>
        /// <br>musi być większa od 0, natomiast dla parametru Mod.NEGATIVE musi być większa od 0 oraz mniejsza od 1 w przeciwnym</br>
        /// <br>wypadku zostanie rzucony wyjątek. Jeżeli parametr attribute będzie NULL to zostanie rzucony wyjątek.</br>
        /// </summary>
        /// <param name="modifer">Wartość modyfikacji</param>
        /// <param name="mod">Tryb modyfikacji</param>
        /// <param name="attribute">Atrbut do modyfikacji</param>
        /// <param name="time">Czas trwania modyfikacji</param>
        public TemplateModifier(float modifer, Mod mod, Attribute attribute, float time) : this( modifer, mod, "", attribute, time )
        {
        }

        /// <summary>
        /// <br>Dodaje czasowy modyfikator do atrybutu. Jeżeli podany czas będzie 0 lub mniejszy, zostanie rzucony wyjątek.</br>
        /// <br>Aby rozpocząć odliczanie należy wywołać metodę StartTimer(). Wielkość modyfikacji dla paremtru Mod.POSITIVE </br>
        /// <br>musi być większa od 0, natomiast dla parametru Mod.NEGATIVE musi być większa od 0 oraz mniejsza od 1 w przeciwnym</br>
        /// <br>wypadku zostanie rzucony wyjątek. Jeżeli parametr attribute będzie NULL to zostanie rzucony wyjątek.</br>
        /// </summary>
        /// <param name="modifer">Wartość modyfikacji</param>
        /// <param name="mod">Tryb modyfikacji</param>
        /// <param name="name">Nazwa modyfikacji</param>
        /// <param name="attribute">Atrbut do modyfikacji</param>
        /// <param name="time">Czas trwania modyfikacji</param>
        public TemplateModifier(float modifer, Mod mod, string name, Attribute attribute, float time) : base( modifer, mod, name, attribute )
        {
            if (time <= 0)
                throw new System.Exception( "TemplateModifier::Constructor::Name::" + modifierName + "::" + "(Modifier time cannot be less than 0!)" );
            this.time = time;
            countdown = TimerManager.StartCountdown( 0, false, this );
            TimerManager.RemoveWhenEnds( countdown, true );
        }

        public void StartTimer()
        {
            Debug.Log( TimerManager.ResetCountdown( countdown, time ) );
        }

        public void OnCountdownEnd(long id)
        {
            attribute.RemoveModifier( this );
        }
    }
}