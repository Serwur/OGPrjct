using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <br>Klasa reprezentująca odliczacz czasu, należy stworzyć jedną jej instancję na scenie, aby z niej poprawnie korzystać.</br>
/// <br>Może być wykorzystywana do odliczania czasu jak sama nazwa wskazuje bez wykorzystywania większej ilości mocy obliczeniowej</br>
/// <br>jeżeli miałoby to miejsce tworząc osobno odliczenia dla każdej zdolności, odliczania czasu do czegoś bądź każdej innej rzeczy</br>
/// <br>wymagającej odstępów czasu między akcjami.</br>
/// <br>Do poprawnego stworzenia nowego czasu odliczania należy użyć metody <code>StartCountdown()</code> oraz przechwycić</br>
/// <br>referencję na zwrócony obiekt typu Countdown.</br>
/// <br>Aby sprawnie otrzymywać komunikat o zakończonym czasie danego obiektu Countdown, należy w swojej klasie zaimplementować</br>
/// <br>interfejs <code>IOnCountdownEnd</code>, metoda <code>OnCountdownEnd()</code> zostanie wywołana jednorazowo w momencie zakończenia </br>
/// <br>czasu odliczenia.</br>
/// </summary>
public class TimerManager : MonoBehaviour
{
    // PUBLIC STATIC
    public static readonly int MINUTE_UNIT = 60;
    public static readonly int HOUR_UNIT = 3600;
    public static readonly int DAY_UNIT = 86400;

    // PRIVATE STATIC
    private static TimerManager Instance;

    // PUBLIC
    [Header( "Properties" )]
    public float startTime = 0f;
    [Range( 0.1f, 5f )]
    public float timeSpeed = 1f;

    // PRIVATE
    private float time = 0f;
    private Dictionary<long, Countdown> endedCountdowns = new Dictionary<long, Countdown>();
    private Dictionary<long, Countdown> notEndedCountdowns = new Dictionary<long, Countdown>();

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Debug.LogError( "Timer::Awake::(Trying to create another TimerManager object!)" );
            Destroy( gameObject );
            return;
        }
        time = startTime;
    }

    private void FixedUpdate()
    {
        Tick( Time.fixedDeltaTime );
    }

    /// <summary>
    /// <br>Metoda powinna być wywoływana metodzie Update() lub FixedUpdate(), a jako argument przekazany czas Time.DeltaTime lub Time.FixedDeltaTime.</br>
    /// <br><b>DO POPRAWNEGO DZIAŁANIA NALEŻY TYLKO I WYŁĄCZNIE W JEDNYM MIEJSCU WYWOŁYWAĆ TĄ METODĘ, W PRZECIWNYM WYPADKU</br>
    /// <br>DOJDZIE DO BŁĘDNEGO ODLICZANIA CZASU</b></br>
    /// </summary>
    /// <param name="passedTime">Czas, który minął w danej klatce lub "fixed" klatce</param>
    private void Tick(float passedTime)
    {
        time += passedTime * timeSpeed;
        LinkedList<long> countdownsToRemove = new LinkedList<long>();
        foreach (long key in notEndedCountdowns.Keys) {
            if (notEndedCountdowns[key].HasEnded()) {
                countdownsToRemove.AddLast( key );
                if (notEndedCountdowns[key].Listener != null) {
                    notEndedCountdowns[key].Listener.OnCountdownEnd( key );
                }
                if (!notEndedCountdowns[key].RemoveWhenEnds)
                    endedCountdowns.Add( key, notEndedCountdowns[key] );
            }
        }
        foreach (long key in countdownsToRemove) {
            notEndedCountdowns.Remove( key );
        }
    }

    public static void SetTime(int seconds)
    {
        SetTime( seconds, 0 );
    }

    public static void SetTime(int seconds, int minutes)
    {
        SetTime( seconds, minutes, 0 );
    }

    public static void SetTime(int seconds, int minutes, int hours)
    {
        Instance.time = seconds + minutes * MINUTE_UNIT + hours * HOUR_UNIT;
    }

    /// <summary>
    /// Starts a new countdown with given time. If time is less that 0 then countdown is marked as ended.
    /// It's important to take <b>id</b> as returned long value.
    /// </summary>
    /// <param name="countdown">Time to countdown</param>
    /// <returns>id of new countdown</returns>
    public static long StartCountdown(float countdown)
    {
        return StartCountdown( countdown, null );
    }

    public static long StartCountdown(float countdown, IOnCountdownEnd listener)
    {
        long id = (long) (Time.time * 1000);
        Countdown _countdown = new Countdown( countdown, listener );
        if (countdown > 0)
            Instance.notEndedCountdowns.Add( id, _countdown );
        else
            Instance.endedCountdowns.Add( id, _countdown );
        return id;
    }

    /// <summary>
    /// Zwraca pozostały czas podanego timera w sekundach
    /// </summary>
    /// <param name="countdown">Timer do otrzymania pozostałego czasu odliczania</param>
    /// <returns>Pozostały czas od końca odliczania w postaci ciągu znaków</returns>
    public static bool GetCountdownSeconds(long id, out string seconds)
    {
        Countdown countdown;
        if (GetCountdown( id, out countdown )) {
            seconds = countdown.InSec();
            return true;
        }
        seconds = null;
        return false;
    }

    /// <summary>
    /// Resetuje timer używając podanego czasu odliczania
    /// </summary>
    /// <param name="countdown">Timer do zresetowania</param>
    /// <param name="newCountdown">Nowy czas odliczania dla timer'a</param>
    public static bool ResetCountdown(long id, float newCountdown)
    {
        Countdown countdown;
        if (GetCountdown( id, out countdown )) {
            if (countdown.HasEnded()) {
                Instance.notEndedCountdowns.Add( id, countdown );
                Instance.endedCountdowns.Remove( id );
            }
            if (newCountdown == -1) {
                countdown.Reset( countdown.CountdownTime );
            } else {
                countdown.Reset( newCountdown );
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Resets the countdown with given id.
    /// </summary>
    /// <param name="id">id of countdown</param>
    /// <returns>TRUE if countdown has been reseted (it means that countdown exists) otherwise returns FALSE</returns>
    public static bool ResetCountdown(long id)
    {
        return ResetCountdown( id, -1 );
    }

    /// <summary>
    /// Checks if countdown with given id has ended. Information about coundown ends is given in out parameter <b>hasEnded</b>
    /// </summary>
    /// <param name="id">id of countdown</param>
    /// <param name="hasEnded">Information if countdown has ended</param>
    /// <returns>TRUE if countdown exists, otherwise it returns FALSE</returns>
    public static bool HasEnded(long id, out bool hasEnded)
    {
        Countdown countdown;
        if (GetCountdown( id, out countdown )) {
            hasEnded = countdown.HasEnded();
            return true;
        }
        hasEnded = false;
        return false;
    }

    /// <summary>
    /// Reset countdown times of all countdowns.
    /// </summary>
    public static void ResetCountdowns()
    {
        ResetNotEndedCountdowns();
        ResetEndedCountdowns();
    }

    /// <summary>
    /// Reset countdown times of all not ended countdowns.
    /// </summary>
    public static void ResetNotEndedCountdowns()
    {
        foreach (Countdown countdown in Instance.notEndedCountdowns.Values) {
            countdown.Reset( countdown.CountdownTime );
        }
    }

    /// <summary>
    /// Reset countdown times of all ended countdowns.
    /// </summary>
    public static void ResetEndedCountdowns()
    {
        foreach (Countdown countdown in Instance.endedCountdowns.Values) {
            countdown.Reset( countdown.CountdownTime );
        }
    }

    /// <summary>
    /// Remove countdown of given id.
    /// </summary>
    /// <param name="id">id of countdown</param>
    /// <returns>TRUE if countdown was removed (it means countdown exists) otherwise returns FALSE.</returns>
    public static bool RemoveCountdown(long id)
    {
        if (Instance.endedCountdowns.Remove( id ))
            return true;
        return Instance.notEndedCountdowns.Remove( id );
    }

    /// <summary>
    /// Sets countdown to remove when ends depdens on given <br>remove</br> argument.
    /// </summary>
    /// <param name="id">id of countdown</param>
    /// <param name="remove"></param>
    /// <returns>>TRUE if field ws set (it means countdown exists) otherwise returns FALSE.</returns>
    public static bool RemoveWhenEnds(long id, bool remove)
    {
        Countdown countdown;
        if (GetCountdown( id, out countdown )) {
            countdown.RemoveWhenEnds = remove;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gives past time in seconds of created singleton.
    /// </summary>
    /// <returns>Gives past time in seconds</returns>
    public static float Seconds()
    {
        return Instance.time;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeSpeed">Time speed to set for countdown</param>
    /// <returns></returns>
    public static bool SetCountdownTimeSpeed(long id, float timeSpeed)
    {
        if (timeSpeed <= 0f)
            throw new System.Exception( "TimerManager::SetCountdownTimeSpeed::(Time speed cannot be less than 0 or equal)" );
        Countdown countdown;
        if (GetCountdown( id, out countdown )) {
            countdown.TimeSpeed = timeSpeed;
            return true;
        }
        return false;
    }

    public static float Minutes()
    {
        return Instance.time / MINUTE_UNIT;
    }

    public static float Hours()
    {
        return Instance.time / HOUR_UNIT;
    }

    public static string InSeconds()
    {
        int seconds = ( (int) Instance.time % MINUTE_UNIT );
        return ( ( seconds < 10 ) ? "0" : "" ) + seconds.ToString();
    }

    public static string InMinutesSeconds()
    {
        int minutes = ( (int) Instance.time % HOUR_UNIT ) / MINUTE_UNIT;
        return ( ( minutes < 10 ) ? "0" : "" ) + minutes.ToString() + ":" + InSeconds();
    }

    public static string InHoursMinutesSeconds()
    {
        int hours = ( (int) Instance.time % DAY_UNIT ) / HOUR_UNIT;
        return ( ( hours < 10 ) ? "0" : "" ) + hours.ToString() + ":" + InMinutesSeconds();
    }

    public static float TimeSpeed
    {
        get {
            return Instance.timeSpeed;
        }

        set {
            if (value <= 0f)
                throw new SystemException( "Time speed cannot be 0 or less!" );
            Instance.timeSpeed = value;
        }
    }

    /// <summary>
    /// Gets countdown from singleton named Instance. If countdown with id doesn't exists it returns null.
    /// </summary>
    /// <param name="id">id of countdown to get</param>
    /// <returns>Countdown with given id</returns>
    private static bool GetCountdown(long id, out Countdown countdown)
    {
        if (Instance.notEndedCountdowns.TryGetValue( id, out countdown )) {
            return true;
        }
        return Instance.endedCountdowns.TryGetValue( id, out countdown );
    }

    private class Countdown
    {
        private float countdownTime = 0f;
        private float countdownStartTime = 0f;
        private float timeSpeed = 1f;
        private IOnCountdownEnd listener = null;
        private bool removeWhenEnds = false;

        internal Countdown(float countdownTime, IOnCountdownEnd listener)
        {
            this.countdownTime = countdownTime;
            this.listener = listener;
            countdownStartTime = Instance.time;
        }

        /// <summary>
        /// Checks if countdown has ended.
        /// </summary>
        /// <returns>
        /// TRUE, if countdown ended otherwise FALSE.
        /// </returns>
        internal virtual bool HasEnded()
        {
            return ( Instance.time - countdownStartTime ) * timeSpeed >= countdownTime;
        }

        /// <summary>
        /// Resets the countdown with new countdown time.
        /// </summary>
        /// <param name="newCountdown">
        /// New countdown time to end.
        /// </param>
        internal void Reset(float newCountdown)
        {
            countdownTime = newCountdown;
            countdownStartTime = Instance.time;
        }

        /// <summary>
        /// Gets time in seconds to end the countdown.
        /// </summary>
        /// <returns>
        /// Time in seconds left to end countdown.
        /// </returns>
        internal int Seconds()
        {
            return (int) ( countdownTime - ( Instance.time - countdownStartTime ) );
        }

        internal string InSec()
        {
            int seconds = Seconds() % MINUTE_UNIT;
            return ( ( seconds < 10 ) ? "0" : "" ) + seconds.ToString();
        }

        public IOnCountdownEnd Listener { get => listener; set => listener = value; }
        public bool RemoveWhenEnds { get => removeWhenEnds; set => removeWhenEnds = value; }
        public float CountdownTime { get => countdownTime; }
        public float CountdownStartTime { get => countdownStartTime; }
        public float TimeSpeed { get => timeSpeed; set => timeSpeed = value; }
    }

    /// <summary>
    /// <br>Interface that should be used in every case when something must happen all the time</br>
    /// <br>even object doesn't need to know when countdown ends.</br>
    /// <br>Important: remember to use Reset method on Countdown object when timer ends!</br>
    /// <br>Example: all units on the map need to regenerate hp or mana in the same time even they</br>
    ///          <br>are stunned, imbollized, disactive etc., then this method will be called all</br>
    ///          <br>the time when countdown ends.</br>
    /// <example>
    /// <code>
    /// void OnCountdownEnd(Countdown countdown) {
    ///     foreach ( Unit unit in units ) {
    ///         unit.Regenerate();
    ///     }
    ///     countdown.Reset();
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public interface IOnCountdownEnd
    {
        void OnCountdownEnd(long id);
    }
}