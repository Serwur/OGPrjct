using UnityEngine;

[RequireComponent( typeof( Rigidbody ) )]
public abstract class Entity : MonoBehaviour
{
    [Header( "Attributes" )]
    public PAttribute hitPoints;
    public PAttribute sourcePoints;
    public PAttribute armor;
    public PAttribute moveSpeed;
    public PAttribute jumpPower;
    public PAttribute damage;

    [Header( "Regeneration" )]
    public float regenHitPoints = 0f;
    public float regenSourcePoints = 0f;

    [Header( "Utility" )]
    /// <summary>
    /// <br>Ważne pole, które informuje nas o tym czy jednostke jest martwa, jeżeli tak to wiele akcji nie jest wykonywane.</br>
    /// <br>Pole jest po to, aby kolejno zdecydować co zrobić z jednostką, nie koniecznie musimy ją usuwać z gry, tylko</br>
    /// <br>wykorzystać w innym potrzebnym etapie.</br>
    /// </summary>
    [SerializeField] protected bool isDead = false;
    [SerializeField] protected bool canMove = true;
    [SerializeField] protected bool isInviolability = false;
    [SerializeField] protected bool isPaused = false;

    [Header( "Dialogs" )]
    public DialogueList[] dialogueLists;

    protected Rigidbody rb;
    protected bool isBlocking = false;

    #region Unity API
    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public virtual void Start()
    {
        GameManager.AddEntity( this );

        UpdateAttributes();
        hitPoints.current = hitPoints.max;
        sourcePoints.current = sourcePoints.max;
        armor.current = armor.max;
        moveSpeed.current = moveSpeed.max;
        jumpPower.current = jumpPower.max;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Zadaje obrażenia jednostce.
    /// </summary>
    /// <param name="attack">Informacje o wykonywanym ataku</param>
    /// <returns>Zwraca TRUE, jeżeli jednostka otrzymała jakiekolwiek obrażenia w przeciwnym wypadku zwraca FALSE</returns>
    public virtual bool TakeDamage(Attack attack)
    {
        if (isInviolability)
            return false;
        if (isBlocking && ShouldBlockAttack( attack )) {
            return false;
        }
        if (attack.damage > 0f) {
            hitPoints.current -= attack.damage;
            if (hitPoints.current < 0f)
                Die();
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// <br>Metoda wywoływana co sekundę w celu regeneracji zycia oraz zasobów.</br>
    /// <br>Powinna być wywołana w jednej klasie, która będzie mieć dostep do wszystkich jednostek np. GameMaster.</br>
    /// </summary>
    public virtual void Regenerate()
    {
        float nextHitPoints = hitPoints.current + regenHitPoints;
        if (nextHitPoints > hitPoints.max)
            hitPoints.current = hitPoints.max;
        else
            hitPoints.current = nextHitPoints;

        float nextSourcePoints = sourcePoints.current + regenSourcePoints;
        if (nextSourcePoints > sourcePoints.max)
            sourcePoints.current = hitPoints.max;
        else
            sourcePoints.current = nextSourcePoints;
    }

    /// <summary>
    /// Metoda lecząca jednostkę o podane punkty zdrowia. Nadpisać metodę w klasach pochodnych, jeżeli istnieje taka potrzeba.
    /// </summary>
    /// <param name="heal">Ilość punktów zdrowia do uleczenia</param>
    /// <returns>TRUE jeżeli zwiększono ilość punktów zdrowia, w przeciwnym wypadku FALSE</returns>
    public virtual bool Heal(float heal)
    {
        float nextHitPoints = hitPoints.current + heal;
        if (nextHitPoints > hitPoints.max)
            hitPoints.current = hitPoints.max;
        else
            hitPoints.current = nextHitPoints;
        return true;
    }

    /// <summary>
    /// <br>Aktualizuje statystyki jednostki takie jak maksymalna ilość zdrowia, maksymalna ilość zasobów, atak, pancerz itp. itd.</br>
    /// <br>Metoda powinna być wywoływana w momencie zmiany którejś ze statystyk zwiększających lub zmniejsząjących obrażenia, życie maksymalne itp..</br>
    /// </summary>
    public virtual void UpdateAttributes()
    {
        hitPoints.UpdateAttribute();
        sourcePoints.UpdateAttribute();
        armor.UpdateAttribute();
        moveSpeed.UpdateAttribute();
        jumpPower.UpdateAttribute();
    }

    /// <summary>
    /// <br>Zamysł tej metody jest taki, że resetuje stan gracza ustawiajac jego hp i zasoby do 100%,</br>
    /// <br>usuwając wszystkie czasowe oraz pasywne buffy/debuffy (wliczajac w to wzmocnienia, stuny itd. itp)</br>
    /// </summary>
    public abstract void ResetUnit();

    /// <summary>
    /// <br>Zabija jednostkę wykonując przy tym zaimplementowane działania np. wykonywanie animacji śmierci, dodanie</br>
    /// <br>punktów graczom itp. itd.</br>
    /// </summary>
    public abstract void Die();

    /// <summary>
    /// Checks if attack should be blocked
    /// </summary>
    /// <returns></returns>
    public abstract bool ShouldBlockAttack(Attack attack);
    #endregion

    #region Setter And Getters
    public bool IsPaused { get => isPaused; set => isPaused = value; }
    #endregion
}
