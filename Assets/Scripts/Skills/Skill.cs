using UnityEngine;

public abstract class Skill : MonoBehaviour, TimerManager.IOnCountdownEnd
{
    [Header( "Visual" )]
    public string skillName;
    public string description;
    public Sprite icon;

    [Header( "Standard properties" )]
    public bool canUse = true;

    private long countdown;

    private void Awake()
    {
        countdown = TimerManager.StartCountdown( 0, this );
    }

    /// <summary>
    /// Wywoływana podczas jednego naciśnięcia przycisku, akcje typu: natychmiastowe, rzucanie pociskami, 
    /// rzucane na siebie, przeciwników, sojuszników, typowe dla jednorazowego użytku co jakiś czas.
    /// </summary>
    public virtual void Use()
    {
        Debug.Log( "Skill::Use::" + skillName );
    }

    /// <summary>
    /// Wywoływana podczas wciśniętego przycisku, akcje typu: ładowanie czaru aż do naładowania,
    /// ładowanie by mógł być mocniejszy przy wypuszczeniu.
    /// </summary>
    /// <param name="deltaTime">Czas, który upłynął w ostatniej klatce</param>
    public virtual void Charge(float deltaTime)
    {
        Debug.Log( "Skill::Charge::" + skillName );
    }

    /// <summary>
    /// Metoda wywołana jednorazowo po puszczeniu przycisku.
    /// </summary>
    public virtual void Release()
    {
        Debug.Log( "Skill::Release::" + skillName );
    }

    protected virtual void CantUse()
    {
        Debug.Log( "Skill::CantUse::" + skillName );
    }

    /// <summary>
    /// W tej metodzie powinna być zaimplementowana akcja kiedy skończy się czas ładowania
    /// zdolności np. efekt wizualny na GUI, jakieś inne bajery, jakieś buffy itd.
    /// </summary>
    /// <param name="id"></param>
    public void OnCountdownEnd(long id)
    {
    }

}
