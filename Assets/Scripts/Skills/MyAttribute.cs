using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <br>Służy jako atrybut dla jednostki, mogą to być punkty zdrowia, mana, siła, zręczność itp. zawiera takie pola</br>
/// <br>jak <code>standard</code>, <code>current</code>, <code>max</code>, które kolejno określają:</br>
/// <br><b>standard:</b> bazowy atrybut bez uwzględniania modyfikatorów</br>
/// <br><b>current:</b> aktualna wartość atrybutu dla jednostki, która powinna być wykorzystywana do określenia</br>
/// <br>aktualnego stanu...</br>
/// </summary>
[System.Serializable]
public class MyAttribute
{
    /// <summary>
    /// Bazowa statystyka bez żadnych modyfikatorów
    /// </summary>
    public float basic = 0;
    /// <summary>
    /// Aktualna staystyka uwzględniając modyfikatory. Nie może przekroczyć maksymalnej statystyki.
    /// </summary>
    public float current = 0;
    /// <summary>
    /// Maksymalna statystyka uwzględniając modyfikatory
    /// </summary>
    public float max = 0;

    /// <summary>
    /// Uwzględnia tylko maksymalny modyfikator tymczasowy, dotyczy to negatywnych jak i pozytywnych
    /// </summary>
    public bool onlyMaxTemplateModifer = false;
    /// <summary>
    /// Uwzględnia tylko maksymalny modyfikator permamentny, dotyczny to negatywnych jak i pozytywnych
    /// </summary>
    public bool onlyMaxPassiveModifer = false;

    /// <summary>
    /// Lista modyfikatorów czasowych atrybutu.
    /// </summary>
    protected LinkedList<TemplateModifier> templateModifers = new LinkedList<TemplateModifier>();
    /// <summary>
    /// Lista modyfikatorów permamentnych atrybutu.
    /// </summary>
    protected LinkedList<PermamentModifier> permamentModifers = new LinkedList<PermamentModifier>();

    /// <summary>
    /// Dodaje modyfikator atrybutu
    /// </summary>
    /// <param name="modifier">Modyfikator przeznaczony do dodania</param>
    public void AddModifier(Modifier modifier)
    {
        if (modifier.GetType() == typeof( TemplateModifier )) {
            templateModifers.AddLast( (TemplateModifier) modifier );
            Debug.Log( "MyAttribute::AddModifier::(Added template modifier " + modifier.modifierName + " for " + ((TemplateModifier) modifier).time + " seconds)" );
        } else {
            permamentModifers.AddLast( (PermamentModifier) modifier );
            Debug.Log( "MyAttribute::AddModifier::(Added permament modifier: " + modifier.modifierName + ")" );
        }
        UpdateAttribute();  
    }

    /// <summary>
    /// Usuwa modyfikator atrybutu
    /// </summary>
    /// <param name="modifier">Modyfiaktor przeznaczony do usunięcia</param>
    public bool RemoveModifier(Modifier modifier)
    {
        bool removed = false;
        if (modifier.GetType() == typeof( TemplateModifier )) {
            removed = templateModifers.Remove( (TemplateModifier) modifier );
        } else {
            removed = permamentModifers.Remove( (PermamentModifier) modifier );
        }
        UpdateAttribute();
        return removed;
    }

    /// <summary>
    /// <br>Metoda do aktualizacja maksymalnego atrybutu po jakiejkolwiek zmianie w modyfikatorach.</br>
    /// <br>Wywoływana jest za każdym wywołaniem metody <code>AddModifier()</code> oraz <code>RemoveModifier()</code></br>
    /// </summary>
    public void UpdateAttribute()
    {
        max = basic;
        float maxTemplate = 0, maxPermament = 0;
      //  GetMaxModifier( new HashSet<Modifier> );
        foreach (TemplateModifier modifier in templateModifers) {
            if ( onlyMaxTemplateModifer ) {

            }
            else
                max *= modifier.GetModify();
        }
        foreach (PermamentModifier modifier in permamentModifers) {
            max *= modifier.GetModify();
        }
        if (current > max)
            current = max;
    }

    protected Modifier GetMaxModifier<T>(HashSet<T> modifiers) where T: Modifier
    {
        return null;
    }
}
