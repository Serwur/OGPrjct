using ColdCry.Exception;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCry.Utility
{

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
        #region Static Fields
        public static readonly int MINUTE_UNIT = 60;
        public static readonly int HOUR_UNIT = 3600;
        public static readonly int DAY_UNIT = 86400;

        private static TimerManager Instance;
        #endregion

        #region Public Fields
        [Header( "Properties" )]
        public float startTime = 0f;
        #endregion

        #region Private Fields
        private float time = 0f;
        private long nextId = 0;
        private Dictionary<long, Countdown> endedCountdowns = new Dictionary<long, Countdown>();
        private Dictionary<long, Countdown> notEndedCountdowns = new Dictionary<long, Countdown>();
        #endregion

        #region Unity API
        private void Awake()
        {
            if (Instance != null) {
                throw new SingletonException( "Cannot create second object of type " + GetType().Name );
            }
            Instance = this;
            time = startTime;
        }

        private void Update()
        {
            Tick( Time.deltaTime );
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets time by given parameters. If one of parameters is less than 0 then it throws exception.
        /// </summary>
        /// <param name="seconds">Time in seconds</param>
        /// <param name="minutes">Time on minutes</param>
        /// <param name="hours">Time in hours</param>
        public static void SetTime(int seconds, int minutes = 0, int hours = 0)
        {
            if (seconds < 0 || minutes < 0 || hours < 0)
                throw new SystemException( "Neither parameter cannot be less than 0!" );
            Instance.time = seconds + minutes * MINUTE_UNIT + hours * HOUR_UNIT;
        }

        /// <summary>
        /// Should be used when just want to gets ID and doesn't want to start countdown at this moment
        /// </summary>
        /// <param name="listener">Listener to listen call back when countdown ends</param>
        /// <param name="removeWhenEnds">If <code><b>True</b></code> countdown will be removed when ends</param>
        /// <returns></returns>
        public static long Create(IOnCountdownEnd listener = null, bool removeWhenEnds = false)
        {
            return Create( 0, listener, removeWhenEnds );
        }

        /// <summary>
        /// Should be used when just want to gets ID and doesn't want to start countdown at this moment
        /// </summary>
        /// <param name="countdown">Countdown time in seconds</param>
        /// <param name="listener">Listener to listen call back when countdown ends</param>
        /// <param name="removeWhenEnds">If <code><b>True</b></code> countdown will be removed when ends</param>
        /// <returns></returns>
        public static long Create(float countdown, IOnCountdownEnd listener = null, bool removeWhenEnds = false)
        {
            if (countdown < 0)
                throw new SystemException( "Countdown time cannot be less than 0!" );
            long id = NextId();
            Instance.endedCountdowns.Add( id, new Countdown( countdown, listener, removeWhenEnds, false ) );
            return id;
        }

        public static long CreateSchedule(float interval, IOnCountdownEnd listener = null)
        {
            return CreateSchedule( interval, listener, -1 );
        }

        public static long CreateSchedule(float interval, int repeats)
        {
            return CreateSchedule( interval, null, repeats );
        }

        public static long CreateSchedule(float interval, IOnCountdownEnd listener, int repeats)
        {
            if (interval < 0)
                throw new SystemException( "Interval time cannot be less than 0!" );
            long id = NextId();
            Instance.endedCountdowns.Add( id, new Countdown( interval, null, false, repeats ) );
            return id;
        }

        /// <summary>
        /// Starts a new countdown with given time. If time is less than 0 then exception is thrown.
        /// It's important to take <b>id</b> as returned long value.
        /// </summary>
        /// <param name="countdown">Time to countdown</param>
        /// <param name="start">True, if should start immidetly</param>
        /// <param name="listener">Listener to listen call back when countdown ends</param>
        /// <param name="removeWhenEnds">If <code><b>True</b></code> countdown will be removed when ends</param>
        /// <returns>id of new countdown</returns>
        public static long Start(float countdown, IOnCountdownEnd listener = null, bool removeWhenEnds = false)
        {
            if (countdown < 0)
                throw new ArgumentException( "Time for countdown cannot be less than 0!" );
            long id = NextId();
            Instance.notEndedCountdowns.Add( id, new Countdown( countdown, listener, removeWhenEnds, false ) );
            return id;
        }

        public static long StartSchedule(float interval, IOnCountdownEnd listener = null)
        {
            return StartSchedule( interval, listener, -1 );
        }

        public static long StartSchedule(float interval, int repeats)
        {
            return StartSchedule( interval, null, repeats );
        }

        public static long StartSchedule(float interval, IOnCountdownEnd listener, int repeats)
        {
            if (interval < 0)
                throw new ArgumentException( "Interval time cannot be less than 0!" );
            long id = NextId();
            Instance.notEndedCountdowns.Add( id, new Countdown( interval, listener, false, repeats ) );
            return id;
        }

        /// <summary>
        /// Returns remaing time of countdown in seconds as <b>string</b>
        /// </summary>
        /// <param name="countdown">Id of countdown</param>
        /// <param name="seconds">Out parameter to get remaing time</param>
        /// <returns><code><b>True</b></code> if given countdown exists, otherwise <code><b>false</b></code></returns>
        public static bool GetRemaing(long id, out string seconds)
        {
            if (GetCountdown( id, out Countdown countdown )) {
                seconds = countdown.InSec;
                return true;
            }
            seconds = null;
            return false;
        }

        /// <summary>
        /// Returns remaing time of countdown in seconds as <b>float</b> value
        /// </summary>
        /// <param name="countdown">Id of countdown</param>
        /// <param name="seconds">Out parameter to get remaing time</param>
        /// <returns><code><b>True</b></code> if given countdown exists, otherwise <code><b>false</b></code></returns>
        public static bool GetRemaing(long id, out float seconds)
        {
            if (GetCountdown( id, out Countdown countdown )) {
                seconds = countdown.Seconds;
                return true;
            }
            seconds = float.MinValue;
            return false;
        }

        /// <summary>
        /// Resets a countdown with given ID and sets a new countdown time if given second parameter.
        /// </summary>
        /// <param name="id">ID of countdown</param>
        /// <param name="newCountdown">New time for countdown</param>
        /// <returns><b>True</b> if countdown has been reseted (it means that countdown exists), otherwise it returns <b>false</b></returns>
        public static bool Reset(long id, float newCountdown = float.MaxValue)
        {
            if (newCountdown < 0)
                throw new SystemException( "Countdown time cannot be less than 0!" );
            if (GetCountdown( id, out Countdown countdown )) {
                countdown.WillBeRemoved = false;
                Instance.endedCountdowns.Remove( id );
                if (!Instance.notEndedCountdowns.ContainsKey( id )) {
                    Instance.notEndedCountdowns.Add( id, countdown );
                }
                if (newCountdown == float.MaxValue) {
                    countdown.Reset( countdown.CountdownTime );
                } else {
                    countdown.Reset( newCountdown );
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if countdown with given id has ended.
        /// </summary>
        /// <param name="id">id of countdown</param>
        /// <returns><b>True</b> if countdown has ended, otherwise returns <b>false</b></returns>
        public static bool HasEnded(long id)
        {
            if (GetCountdown( id, out Countdown countdown )) {
                return countdown.HasEnded;
            }
            return false;
        }

        /// <summary>
        /// Checks if countdown with given id is scheduled.
        /// </summary>
        /// <param name="id">id of countdown</param>
        /// <returns><b>True</b> if countdown is scheduled, otherwise returns <b>false</b></returns>
        public static bool IsScheduled(long id)
        {
            if (GetCountdown( id, out Countdown countdown )) {
                return countdown.IsScheduled;
            }
            return false;
        }

        /// <summary>
        /// Pauses/resumes countdown
        /// </summary>
        /// <param name="id">ID of affected countdown</param>
        /// <param name="pause"><b>True</b> to pause, <b>false</b> to resume</param>
        /// <returns><b>True</b> if countdown has been found, otherwise <b>false</b></returns>
        public static bool Pause(long id, bool pause)
        {
            if (GetCountdownNotEnded( id, out Countdown countdown )) {
                countdown.Paused = pause;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops countdown with given id. This method doesn't call listeners.
        /// </summary>
        /// <param name="id">Id of countdown</param>
        /// <returns><b>True</b> if countdown exists, otherwise it returns <b>false</b></returns>
        public static bool Stop(long id)
        {
            if (GetCountdownNotEnded( id, out Countdown countdown )) {
                Instance.notEndedCountdowns.Remove( id );
                countdown.Paused = false;
                if (!Instance.endedCountdowns.ContainsKey( id )) {
                    Instance.endedCountdowns.Add( id, countdown );
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reset countdown times of all countdowns.
        /// </summary>
        public static void ResetAll()
        {
            ResetNotEnded();
            ResetEnded();
        }

        /// <summary>
        /// Reset countdown times of all not ended countdowns.
        /// </summary>
        public static void ResetNotEnded()
        {
            foreach (Countdown countdown in Instance.notEndedCountdowns.Values) {
                countdown.Reset( countdown.CountdownTime );
            }
        }

        /// <summary>
        /// Reset countdown times of all ended countdowns.
        /// </summary>
        public static void ResetEnded()
        {
            foreach (Countdown countdown in Instance.endedCountdowns.Values) {
                countdown.Reset( countdown.CountdownTime );
            }
        }

        /// <summary>
        /// Remove countdown of given id.
        /// </summary>
        /// <param name="id">id of countdown</param>
        /// <returns><b>True</b> if countdown was removed (it means countdown exists) otherwise returns <b>false</b>.</returns>
        public static bool Destroy(long id)
        {
            if (Instance.endedCountdowns.Remove( id ))
                return true;
            return Instance.notEndedCountdowns.Remove( id );
        }

        [Obsolete( "Doesn't used anymore" )]
        /// <summary>
        /// Sets countdown to remove when ends depdens on given <br>remove</br> argument.
        /// </summary>
        /// <param name="id">id of countdown</param>
        /// <param name="remove"></param>
        /// <returns>><b>True</b> if field ws set (it means countdown exists) otherwise returns <b>false</b>.</returns>
        public static bool DestroyWhenEnds(long id, bool remove)
        {
            if (GetCountdown( id, out Countdown countdown )) {
                countdown.DestroyWhenEnds = remove;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets time speed represented how fast time passes for specified countdown.
        /// </summary>
        /// <param name="timeSpeed">Time speed to set for countdown</param>
        /// <returns><b>True</b> if countdown exists, otherwise it returns <b>false</b></returns>
        public static bool SetSpeed(long id, float timeSpeed)
        {
            if (timeSpeed <= 0f)
                throw new System.Exception( "TimerManager::SetCountdownTimeSpeed::(Time speed cannot be less than 0 or equal)" );
            if (GetCountdown( id, out Countdown countdown )) {
                countdown.TimeSpeed = timeSpeed;
                return true;
            }
            return false;
        }
        #endregion

        #region Private Methods
        // Main method
        private void Tick(float passedTime)
        {
            time += passedTime;

            LinkedList<long> countdownsToRemove = new LinkedList<long>();

            foreach (long key in notEndedCountdowns.Keys) {
                Countdown countdown = notEndedCountdowns[key];

                if (countdown.Paused)
                    continue;

                float overtime = countdown.Overtime;
                if (overtime >= 0) {
                    countdown.WillBeRemoved = true;

                    if (countdown.Listener != null)
                        countdown.Listener.OnCountdownEnd( key, overtime );

                    if (countdown.IsScheduled) {
                        if (countdown.Repeats <= 0) {
                            countdown.Reset( countdown.CountdownTime );
                            countdown.WillBeRemoved = false;
                        } else {
                            countdown.CurrentRepeat--;
                            if (countdown.CurrentRepeat >= countdown.Repeats) {
                                countdown.Reset( countdown.CountdownTime );
                                countdown.WillBeRemoved = false;
                            }
                        }
                    }

                }

                if (countdown.WillBeRemoved) {
                    countdownsToRemove.AddLast( key );
                    endedCountdowns.Add( key, countdown );
                }
            }

            foreach (long key in countdownsToRemove) {
                notEndedCountdowns.Remove( key );
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

        private static bool GetCountdownNotEnded(long id, out Countdown countdown)
        {
            return Instance.notEndedCountdowns.TryGetValue( id, out countdown );
        }

        /// <summary>
        /// Generates new id
        /// </summary>
        /// <returns>Unique ID</returns>
        private static long NextId()
        {
            return Instance.nextId++;
        }
        #endregion

        #region Getters And Setters
        /// <summary>
        /// Time in seconds represented in format: 00
        /// </summary>
        public static string InSeconds
        {
            get {
                int seconds = ( (int) Instance.time % MINUTE_UNIT );
                return ( ( seconds < 10 ) ? "0" : "" ) + seconds.ToString();
            }
        }
        /// <summary>
        /// Time in minutes and seconds represented in format: 00:00
        /// </summary>
        public static string InMinutesSeconds
        {
            get {
                int minutes = ( (int) Instance.time % HOUR_UNIT ) / MINUTE_UNIT;
                return ( ( minutes < 10 ) ? "0" : "" ) + minutes.ToString() + ":" + InSeconds;
            }
        }
        /// <summary>
        /// Time in hours, minutes and seconds represented in format: 00:00:00
        /// </summary>
        public static string InHoursMinutesSeconds
        {
            get {
                int hours = ( (int) Instance.time % DAY_UNIT ) / HOUR_UNIT;
                return ( ( hours < 10 ) ? "0" : "" ) + hours.ToString() + ":" + InMinutesSeconds;
            }
        }
        /// <summary>
        /// Gives past time in seconds of created singleton.
        /// </summary>
        /// <returns>Gives past time in seconds</returns>
        public static float Seconds => Instance.time;
        public static float Minutes => Instance.time / MINUTE_UNIT;
        public static float Hours => Instance.time / HOUR_UNIT;
        public static int CountNotEndedCountdowns => Instance.notEndedCountdowns.Count;
        public static int CountEndedCountdowns => Instance.endedCountdowns.Count;
        #endregion

        private class Countdown
        {
            public Countdown(float countdownTime, IOnCountdownEnd listener, bool destroyWhenEnds)
                : this( countdownTime, listener, destroyWhenEnds, false )
            { }

            public Countdown(float countdownTime, IOnCountdownEnd listener, bool destroyWhenEnds, bool isScheduled)
            {
                CountdownTime = countdownTime;
                Listener = listener;
                DestroyWhenEnds = destroyWhenEnds;
                IsScheduled = isScheduled;
                CountdownStartTime = Instance.time;
            }

            public Countdown(float countdownTime, IOnCountdownEnd listener, bool destroyWhenEnds, int repeats)
            {
                CountdownTime = countdownTime;
                Listener = listener;
                DestroyWhenEnds = destroyWhenEnds;
                IsScheduled = true;
                Repeats = repeats;
                CountdownStartTime = Instance.time;
            }

            /// <summary>
            /// Resets the countdown with new countdown time.
            /// </summary>
            /// <param name="newCountdown">
            /// New countdown time to end.
            /// </param>
            public void Reset(float newCountdown)
            {
                CountdownTime = newCountdown;
                CountdownStartTime = Instance.time;
            }

            /// <summary>
            /// Checks if countdown has ended.
            /// </summary>
            /// <returns>
            /// <b>True</b>, if countdown ended otherwise <b>false</b>.
            /// </returns>
            public bool HasEnded => ( Instance.time - CountdownStartTime ) * TimeSpeed >= CountdownTime;
            public float Overtime => ( Instance.time - CountdownStartTime ) * TimeSpeed - CountdownTime;
            /// <summary>
            /// Gets time in seconds to end the countdown.
            /// </summary>
            /// <returns>
            /// Time in seconds left to end countdown.
            /// </returns>
            public float Seconds => ( CountdownTime - ( Instance.time - CountdownStartTime ) );
            public string InSec
            {
                get {
                    int seconds = (int) ( Seconds % MINUTE_UNIT );
                    bool lessThanZero = seconds < 0;
                    return ( lessThanZero ? "-" : "" ) + ( ( seconds < 10 ) ? "0" : "" ) + ( lessThanZero ? -seconds : seconds ).ToString();
                }
            }

            public IOnCountdownEnd Listener { get; set; }
            public bool DestroyWhenEnds { get; set; }
            public float CountdownTime { get; private set; }
            public float CountdownStartTime { get; private set; } = 0f;
            public float TimeSpeed { get; set; } = 1f;
            public bool IsScheduled { get; set; }
            public bool WillBeRemoved { get; set; } = false;
            public int Repeats { get; set; } = -1;
            public int CurrentRepeat { get; set; } = 0;
            public bool Paused { get; set; } = false;
        }
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
        void OnCountdownEnd(long id, float overtime);
    }
}