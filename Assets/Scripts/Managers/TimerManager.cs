using ColdCry.Exception;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCry.Utility.Time
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

        private static long CURRENT_ID = 0;

        private static TimerManager Instance;
        #endregion

        #region Public Fields
        [Header( "Properties" )]
        public float startTime = 0f;
        #endregion

        #region Private Fields
        private float time = 0f;
        private Dictionary<long, ICountdown> endedCountdowns = new Dictionary<long, ICountdown>();
        private Dictionary<long, ICountdown> notEndedCountdowns = new Dictionary<long, ICountdown>();
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

        private void Update() => Tick( UnityEngine.Time.deltaTime );
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
        /// Should be used when just want to gets ID and doesn't want to start time at this moment
        /// </summary>
        /// <param name="time">Countdown time in seconds</param>
        /// <param name="listener">Listener to listen call back when time ends</param>
        /// <param name="removeWhenEnds">If <code><b>True</b></code> time will be removed when ends</param>
        /// <returns></returns>
        public static ICountdown Create(float time = 1f, Action<float> onEndAction = null)
        {
            return Countdown.GetInstance( time, onEndAction );
        }

        public static ICountdown CreateSchedule(float interval = 1f, int repeats = -1, Action<float> onEndAction = null)
        {
            return ScheduledCountdown.GetInstance( interval, repeats, onEndAction );
        }

        /// <summary>
        /// Starts a new time with given time. If time is less than 0 then exception is thrown.
        /// It's important to take <b>id</b> as returned long value.
        /// </summary>
        /// <param name="time">Time to time</param>
        /// <param name="start">True, if should start immidetly</param>
        /// <param name="listener">Listener to listen call back when time ends</param>
        /// <param name="removeWhenEnds">If <code><b>True</b></code> time will be removed when ends</param>
        /// <returns>id of new time</returns>
        public static ICountdown Start(float time = 1f, Action<float> onEndAction = null)
        {
            ICountdown countdown = Create( time, onEndAction );
            countdown.Start();
            return countdown;
        }

        public static ICountdown StartSchedule(float interval = 1f, int repeats = -1, Action<float> onEndAction = null)
        {
            ICountdown countdown = CreateSchedule( interval, repeats, onEndAction );
            countdown.Start();
            return countdown;
        }

        /*
        /// <summary>
        /// Returns remaing time of time in seconds as <b>string</b>
        /// </summary>
        /// <param name="time">Id of time</param>
        /// <param name="seconds">Out parameter to get remaing time</param>
        /// <returns><code><b>True</b></code> if given time exists, otherwise <code><b>false</b></code></returns>
        public static bool GetRemaing(long id, out string seconds)
        {
            return GetRemaing( id, seconds.ToString() );
        }*/

        /// <summary>
        /// Returns remaing time of time in seconds as <b>float</b> value
        /// </summary>
        /// <param name="time">Id of time</param>
        /// <param name="seconds">Out parameter to get remaing time</param>
        /// <returns><code><b>True</b></code> if given time exists, otherwise <code><b>false</b></code></returns>
        public static bool GetRemaing(long id, out float seconds)
        {
            if (GetCountdown( id, out ICountdown time )) {
                seconds = time.Remaing;
                return true;
            }
            seconds = float.MinValue;
            return false;
        }

        /// <summary>
        /// Restarts time with given ID and sets a new time time if given second parameter.
        /// </summary>
        /// <param name="id">ID of time</param>
        /// <returns><b>True</b> if time has been reseted (it means that time exists), otherwise it returns <b>false</b></returns>
        public static bool Restart(long id)
        {
            if (GetCountdown( id, out ICountdown countdown )) {
                countdown.Restart();
                return true;
            }
            return false;
        }

        public static bool Restart(long id, float time)
        {
            if (GetCountdown( id, out ICountdown countdown )) {
                countdown.Restart(time);
                return true;
            }
            return false;
        }

        public static bool Restart(long id, float time, int repeats)
        {
            if (GetCountdown( id, out ICountdown countdown )) {
                if ( IsScheduled(countdown) ) {
                    ScheduledCountdown scheduledCountdown = countdown as ScheduledCountdown;
                    scheduledCountdown.Restart( time, repeats );
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if time with given id has ended.
        /// </summary>
        /// <param name="id">id of time</param>
        /// <returns><b>True</b> if time has ended, otherwise returns <b>false</b></returns>
        public static bool HasEnded(long id)
        {
            if (GetCountdown( id, out ICountdown time )) {
                return time.HasEnded();
            }
            return false;
        }

        /// <summary>
        /// Checks if time with given id is scheduled.
        /// </summary>
        /// <param name="id">id of time</param>
        /// <returns><b>True</b> if time is scheduled, otherwise returns <b>false</b></returns>
        public static bool IsScheduled(long id)
        {
            if (GetCountdown( id, out ICountdown time )) {
                return time.GetType() == typeof( ScheduledCountdown );
            }
            return false;
        }

        public static bool IsScheduled(ICountdown time)
        {
            return time.GetType() == typeof( ScheduledCountdown );
        }

        /// <summary>
        /// Pauses/resumes time
        /// </summary>
        /// <param name="id">ID of affected time</param>
        /// <returns><b>True</b> if time has been found, otherwise <b>false</b></returns>
        public static bool Pause(long id)
        {
            if (GetCountdownNotEnded( id, out ICountdown countdown )) {
                countdown.Pause();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops time with given id. This method doesn't call listeners.
        /// </summary>
        /// <param name="id">Id of time</param>
        /// <returns><b>True</b> if time exists, otherwise it returns <b>false</b></returns>
        public static bool Stop(long id)
        {
            if (GetCountdownNotEnded( id, out ICountdown countdown )) {
                countdown.Stop();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reset time times of all countdowns.
        /// </summary>
        public static void RestartAll()
        {
            RestartNotEnded();
            RestartEnded();
        }

        /// <summary>
        /// Reset time times of all not ended countdowns.
        /// </summary>
        public static void RestartNotEnded()
        {
            foreach (ICountdown countdown in Instance.notEndedCountdowns.Values) {
                countdown.Restart();
            }
        }

        /// <summary>
        /// Reset time times of all ended countdowns.
        /// </summary>
        public static void RestartEnded()
        {
            foreach (ICountdown countdown in Instance.endedCountdowns.Values) {
                countdown.Restart( );
            }
        }

        /// <summary>
        /// Remove time of given id.
        /// </summary>
        /// <param name="id">id of time</param>
        /// <returns><b>True</b> if time was removed (it means time exists) otherwise returns <b>false</b>.</returns>
        public static bool Destroy(long id)
        {
            if (Instance.endedCountdowns.Remove( id ))
                return true;
            return Instance.notEndedCountdowns.Remove( id );
        }

        /*
        [Obsolete( "Doesn't used anymore" )]
        /// <summary>
        /// Sets time to remove when ends depdens on given <br>remove</br> argument.
        /// </summary>
        /// <param name="id">id of time</param>
        /// <param name="remove"></param>
        /// <returns>><b>True</b> if field ws set (it means time exists) otherwise returns <b>false</b>.</returns>
        public static bool DestroyWhenEnds(long id, bool remove)
        {
            if (GetCountdown( id, out Countdown time )) {
                time.DestroyWhenEnds = remove;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets time speed represented how fast time passes for specified time.
        /// </summary>
        /// <param name="timeSpeed">Time speed to set for time</param>
        /// <returns><b>True</b> if time exists, otherwise it returns <b>false</b></returns>
        public static bool SetSpeed(long id, float timeSpeed)
        {
            if (timeSpeed <= 0f)
                throw new System.Exception( "TimerManager::SetCountdownTimeSpeed::(Time speed cannot be less than 0 or equal)" );
            if (GetCountdown( id, out Countdown time )) {
                time.TimeSpeed = timeSpeed;
                return true;
            }
            return false;
        }*/
        #endregion

        #region Private Methods
        // Main method
        private void Tick(float passedTime)
        {
            time += passedTime;

            LinkedList<ICountdown> countdownsToRemove = new LinkedList<ICountdown>();

            foreach (ICountdown countdown in notEndedCountdowns.Values) {

                if (countdown.Paused)
                    continue;

                if (countdown.HasEnded()) {
                    countdown.OnEnd();
                    if (!countdown.ShouldRestart()) {
                        countdownsToRemove.AddLast( countdown );
                    }
                }
            }

            foreach (ICountdown time in countdownsToRemove) {
                notEndedCountdowns.Remove( time.ID );
                if (!endedCountdowns.ContainsKey( time.ID ))
                    endedCountdowns.Add( time.ID, time );
            }
        }

        /// <summary>
        /// Gets time from singleton named Instance. If time with id doesn't exists it returns null.
        /// </summary>
        /// <param name="id">id of time to get</param>
        /// <returns>Countdown with given id</returns>
        private static bool GetCountdown(long id, out ICountdown time)
        {
            if (Instance.notEndedCountdowns.TryGetValue( id, out time )) {
                return true;
            }
            return Instance.endedCountdowns.TryGetValue( id, out time );
        }

        private static bool GetCountdownNotEnded(long id, out ICountdown time)
        {
            return Instance.notEndedCountdowns.TryGetValue( id, out time );
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


        private static long NextID()
        {
            return CURRENT_ID++;
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

        /// <summary>
        /// Gets time in seconds to end the time.
        /// </summary>
        /// <returns>
        /// Time in seconds left to end time.
        /// </returns>
        /*  public float Seconds => ( CountdownTime - ( Instance.time - CountdownStartTime ) );
          public string InSec
          {
              get {
                  int seconds = (int) ( Seconds % MINUTE_UNIT );
                  bool lessThanZero = seconds < 0;
                  return ( lessThanZero ? "-" : "" ) + ( ( seconds < 10 ) ? "0" : "" ) + ( lessThanZero ? -seconds : seconds ).ToString();
              }
          }*/

        public class Countdown : ICountdown
        {
            private bool pause = false;
            private float timeWhenPaused = 0f;

            protected Countdown()
            {

            }

            protected Countdown(long id, float time, Action<float> onEndAction)
            {
                ID = id;
                Time = time;
                StartTime = Instance.time;
                EndTime = Instance.time + Time;
                OnEndAction = onEndAction;
            }

            public static Countdown GetInstance(float time = 1f, Action<float> onEndAction = null)
            {
                if (Instance == null) {
                    throw new MissingEssentialGameObject( "Cannot create time without active TimerManager object on scene" );
                }
                if (time < 0)
                    throw new SystemException( "Countdown time cannot be less than 0!" );
                Countdown countdown = new Countdown( NextID(), time, onEndAction );
                Instance.endedCountdowns.Add( countdown.ID, countdown );
                return countdown;
            }

            public virtual void Start()
            {
                if (Paused) {
                    StartTime += Instance.time - timeWhenPaused;
                    EndTime += Instance.time - timeWhenPaused;
                } else {
                    StartTime = Instance.time;
                    EndTime = Instance.time + Time;
                }
                pause = false;
                timeWhenPaused = 0;
                if (!Instance.notEndedCountdowns.ContainsKey( ID )) {
                    Instance.notEndedCountdowns.Add( ID, this );
                    Instance.endedCountdowns.Remove( ID );
                }
            }

            public virtual void Restart()
            {
                Restart( Time );
            }

            public virtual void Restart(float time)
            {
                Time = time;
                pause = false;
                timeWhenPaused = 0;
                StartTime = Instance.time;
                EndTime = Instance.time + Time;
                if (!Instance.notEndedCountdowns.ContainsKey( ID )) {
                    Instance.notEndedCountdowns.Add( ID, this );
                    Instance.endedCountdowns.Remove( ID );
                }
            }

            public virtual void Pause()
            {
                if (Instance.notEndedCountdowns.ContainsKey( ID ) && !pause) {
                    timeWhenPaused = Instance.time;
                    pause = true;
                }
            }

            public virtual void Stop()
            {
                pause = false;
                timeWhenPaused = 0;
                Instance.notEndedCountdowns.Remove( ID );
                if (!Instance.endedCountdowns.ContainsKey( ID )) {
                    Instance.endedCountdowns.Add( ID, this );
                }
            }

            public virtual bool HasEnded()
            {
                return EndTime <= Instance.time;
            }

            public virtual void OnEnd()
            {
                if (OnEndAction != null) {
                    OnEndAction.Invoke( Mathf.Abs( EndTime - Instance.time ) );
                }
                foreach (IObserver<ICountdown> observer in Observers) {
                    observer.Notify( this );
                }
            }

            public virtual bool ShouldRestart()
            {
                return EndTime > Instance.time;
            }

            public virtual void Subscribe(IObserver<ICountdown> observer)
            {
                Observers.AddLast( observer );
            }

            public virtual void Unsubscribe(IObserver<ICountdown> observer)
            {
                Observers.Remove( observer );
            }

            public Action<float> OnEndAction { get; set; }
            public float Remaing => pause ? EndTime - timeWhenPaused : EndTime - Instance.time;
            public float Time { get; protected set; }
            public float StartTime { get; protected set; }
            public float EndTime { get; protected set; }
            public bool Paused => pause;
            public long ID { get; set; }
            protected LinkedList<IObserver<ICountdown>> Observers { get; set; } = new LinkedList<IObserver<ICountdown>>();
        }


        public class ScheduledCountdown : Countdown
        {
            private bool pause = false;
            private float timeWhenPaused = 0f;

            protected ScheduledCountdown(long id, float time, int repeats, Action<float> onEndAction) : base( id, time, onEndAction )
            {
                Repeats = repeats;
            }

            public static ScheduledCountdown GetInstance(float time = 1f, int repeats = 0, Action<float> onEndAction = null)
            {
                if (Instance == null) {
                    throw new MissingEssentialGameObject( "Cannot create time without active TimerManager object on scene" );
                }
                if (time < 0)
                    throw new SystemException( "Countdown time cannot be less than 0!" );
                ScheduledCountdown countdown = new ScheduledCountdown( NextID(), time, repeats, onEndAction );
                Instance.endedCountdowns.Add( countdown.ID, countdown );
                return countdown;
            }

            public override void Restart()
            {
                Restart( Time );
            }

            public override void Restart(float time)
            {
                Restart( time, -1 );
            }

            public void Restart(float time, int repeats)
            {
                CurrentRepeat = Repeats;
                base.Restart( time );
            }

            public override void OnEnd()
            {
                base.OnEnd();
                CurrentRepeat--;
            }

            public override bool ShouldRestart()
            {
                if (Repeats <= 0)
                    return true;
                return CurrentRepeat > 0;
            }

            public int Repeats { get; internal set; }
            public int CurrentRepeat { get; private set; }
        }
    }
}