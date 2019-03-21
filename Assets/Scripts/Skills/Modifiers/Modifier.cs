/// <summary>
/// <br>Klasa przeznaczona do sprawnego modyfikowania statystyk postaci/przeciwników. Używa się jej</br>
/// <br>klas bazowych, którymi są <code>PermamentModifier</code> oraz <code>TemplateModifier</code>.</br>
/// <br>Jak same nazwy mówią pierwszy modifier działa przez cały okres czasu życia jednostki,</br>
/// <br>natomiast drugi znika po określonym czasie.</br>
/// </summary>
public abstract class Modifier
{
    /// <summary>
    /// Tryb modyfikatora, pozytywny zwiększający atrybut, negatywny zmniejsząjacy atrybut
    /// </summary>
    public enum Mod
    {
        POSITIVE, NEGATIVE
    }

    public PAttribute attribute;
    public float modifer;
    public string modifierName;
    public Mod mod;

    public Modifier(float modifer, Mod mod, PAttribute attribute) : this(modifer, mod, "", attribute)
    {  }

    /// <summary>
    /// <br>Konstruktor do tworzenia modyfikatora.</br>
    /// </summary>
    /// <param name="modifer">Gdy parametr <b>mod</b> to <b>POSITIVE</b>, wtedy ten parametr musi być większy od 0,
    /// natomiast w przypadku <b>NEGATIVE</b> parametr ten powinien być większy od 0 i mniejszy od 1</param>
    /// <param name="mod">Tryb modyfikatora <b>POSITIVE</b> lub <b>NEGATIVE</b></param>
    /// <param name="modifierName">Nazwa modyfikatora</param>
    /// <param name="attribute">Atrybut do którego będzie przypisany modyfikator, jeżeli parametr ten nie jest <b>NULL</b>
    /// wtedy modyfikator jest automatycznie dodany do atrybutu</param>
    public Modifier(float modifer, Mod mod, string modifierName, PAttribute attribute)
    {
        if (modifer < 0) {
            throw new System.Exception( "Modifier::Constructor::Name::" + modifierName + "::" + "(Modifier value cannot be less than 0!)" );
        }
        if (mod == Mod.NEGATIVE && modifer >= 1) {
            throw new System.Exception( "Modifier::Constructor::Name::" + modifierName + "::" + "(Modifier value with MOD::NEGATIVE cannot be bigger or equal 1.0!)" );
        }
        this.modifer = modifer;
        this.mod = mod;
        this.modifierName = modifierName;
        this.attribute = attribute ?? throw new System.Exception( "Modifier::Constructor::Name::" + modifierName + "::" + "(Attribute cannot be null!)" );
        attribute.AddModifier( this );
    }

    /// <summary>
    /// Zwraca odpowiednią wartość modyfikatora w zależności od jego wartości oraz pola <b>Mod</b>
    /// </summary>
    /// <returns>Wartość modyfikacji atrybutu</returns>
    public float GetModify()
    {
        if (mod == Mod.POSITIVE)
            return ( 1 + modifer );
        return ( 1 - modifer );
    }
}
