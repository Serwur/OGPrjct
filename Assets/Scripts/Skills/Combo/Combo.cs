using Inputs;
using System;
using System.Collections.Generic;

namespace DoubleMMPrjc
{
    public class Combo : IComparable<Combo>
    {
        #region Private Fields
        private Character character;
        private string name;
        private ButtonCode[] comboButtons;
        public Action action;
        #endregion

        /// <summary>
        /// PL: Najważniejsze abyś wiedział jak korzystać z klasy Action. Argument <b>action</b>
        /// działa w ten sposób, że przekazujesz tak jakby metodę, która zostanie wywołana kiedy
        /// combos będzie sie zgadzać (z tym co nacisnął gracz). Minimalnie 3 przyciski w combo.
        /// Wzór:
        /// <code>
        /// Combo combo = new Combo(this, "Super wombo combo", 
        ///             new ButtonCode[] {ButtonCode.A, ButtonCode.B, ButtonCode.X},
        ///             () => { 
        ///                 Tutaj kod metody pisany jak normalna metoda np.
        ///                 SimpleAttack();
        ///                 int x = 3;
        ///                 for ( int i = 0; i < 3; i++ ) {
        ///                     Sraj();
        ///                     if ( i == 2 )
        ///                         break;
        ///                 }
        ///             }
        /// </code>
        /// </summary>
        /// <param name="character"></param>
        /// <param name="name"></param>
        /// <param name="comboButtons"></param>
        /// <param name="action"></param>
        public Combo(Character character, string name, ButtonCode[] comboButtons, Action action)
        {
            if (comboButtons.Length < 3)
                throw new SystemException( "Combo::Constructor::(comboButtons Length cannot be less than 3!)" );
            this.character = character;
            this.name = name;
            this.comboButtons = comboButtons;
            this.action = action;
        }

        /// <summary>
        /// PL: Sprawdza czy lista przycisków ostatnio naciśniętych przez gracza
        /// zgadza się z danym combem.
        /// </summary>
        /// <param name="currentCombination">Aktualna kombinacja przycisków wprowadzona przez gracza</param>
        /// <returns>TRUE jeżeli się zgadza w innym wypadku zwraca FALSE</returns>
        public bool IsValid(LinkedList<ButtonCode> currentCombination)
        {
            if (currentCombination.Count < comboButtons.Length)
                return false;
            int i = 0;
            int j = 0;
            foreach (ButtonCode buttonCode in currentCombination) {
                if (i == comboButtons.Length)
                    return false;
                if (comboButtons[i] == buttonCode) {
                    i++;
                    if (i == comboButtons.Length - 1)
                        break;
                } else if (currentCombination.Count - ( j + 1 ) < comboButtons.Length)
                    return false;
                else
                    i = 0;
                j++;
            }
            return true;
        }

        public override string ToString()
        {
            string combos = "";
            foreach (ButtonCode code in comboButtons) {
                combos += code.ToString();
            }
            return "Combo: " + name + ", combination: " + combos;
        }

        /// <summary>
        /// Metoda z interfejsu, która wykorzystywana jest przy wywołaniu metody sort()
        /// z klasy List, służy oczywiście do sortowania.
        /// </summary>
        /// <param name="x">Pierwszy obiekt klasy Combo</param>
        /// <param name="y">Drugi obiekt klasy Combo</param>
        /// <returns>Jeżeli zwróci liczbę większa od 0 to znaczy, że x jest większy niżeli
        /// y, jeżeli zwróci 0 to są one równe, w innych przypadkach y jest większy od x.</returns>
        public int CompareTo(Combo other)
        {
            return other.comboButtons.Length - comboButtons.Length;
        }
    }
}