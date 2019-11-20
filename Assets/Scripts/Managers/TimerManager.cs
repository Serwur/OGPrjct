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
        private bool isTicking = false;
        // private Dictionary<long, ICountdown> inNextFrame = new Dictionary<long, ICountdown>();
        // private Dictionary<long, ICountdown> unactive = new Dictionary<long, ICountdown>();

        private Dictionary<long, CountdownOperator> toCheckInNextFrame = new Dictionary<long, CountdownOperator>();
        private Dictionary<long, CountdownOperator> ended = new Dictionary<long, CountdownOperator>();
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
                throw new ArgumentException( "Neither parameter cannot be less than 0!" );

            float newTime = seconds + minutes * MINUTE_UNIT + hours * HOUR_UNIT;
            float difference = Instance.time - newTime;
            Instance.time = newTime;

            foreach (CountdownOperator @operator in Instance.toCheckInNextFrame.Values) {
                @operator.StartTime += difference;
                @operator.EndTime += difference;
                if (@operator.Paused) {
                    @operator.PauseTime += difference;
                }
            }
        }

        /// <summary>
        /// Should be used when just want to gets ID and doesn't want to start time at this moment
        /// </summary>
        /// <param name="time">Countdown time in seconds</param>
        /// <param name="listener">Listener to listen call back when time ends</param>
        /// <param name="removeWhenEnds">If <code><b>True</b></code> time will be removed when ends</param>
        /// <returns></returns>
        public static ICountdown Create(float time = 1f, Action<float> action = null)
        {
            return Countdown.GetInstance( time, action );
        }

        public static ICountdown CreateSchedule(float interval = 1f, int repeats = -1, Action<float> action = null)
        {
            return ScheduledCountdown.GetInstance( interval, repeats, action );
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
        public static ICountdown Start(float time = 1f, Action<float> action = null)
        {
            ICountdown countdown = Create( time, action );
            countdown.Restart();
            return countdown;
        }

        public static ICountdown StartSchedule(float interval = 1f, int repeats = -1, Action<float> action = null)
        {
            ICountdown countdown = CreateSchedule( interval, repeats, action );
            countdown.Restart();
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
        /// <param name="id">Id of time</param>
        /// <param name="seconds">Out parameter to get remaing time</param>
        /// <returns><code><b>True</b></code> if given time exists, otherwise <code><b>false</b></code></returns>
        public static bool GetRemaing(long id, out float seconds)
        {
            if (GetCountdown( id, out CountdownOperator cntOperator )) {
                seconds = cntOperator.Remaing;
                return true;
            }
            seconds = -1f;
            return false;
        }

        /// <summary>
        /// Restarts time with given ID and sets a new time time if given second parameter.
        /// </summary>
        /// <param name="id">ID of time</param>
        /// <returns><b>True</b> if time has been reseted (it means that time exists), otherwise it returns <b>false</b></returns>
        [Obsolete( "Use same method on Countdown object instead" )]
        public static bool Restart(long id)
        {
            if (GetCountdown( id, out CountdownOperator cntOperator )) {
                cntOperator.Restart();
                return true;
            }
            return false;
        }

        [Obsolete( "Use same method on Countdown object instead" )]
        public static bool Restart(long id, float time)
        {
            if (GetCountdown( id, out CountdownOperator cntOperator )) {
                cntOperator.Restart( time );
                return true;
            }
            return false;
        }

        [Obsolete( "Use same method on Countdown object instead" )]
        public static bool Restart(long id, float time, int repeats)
        {
            if (GetCountdown( id, out CountdownOperator cntOperator )) {
                if (IsScheduled( cntOperator )) {
                    ( cntOperator as ScheduledCountdownOperator ).Restart( time, repeats );
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
        [Obsolete( "Use same method on Countdown object instead" )]
        public static bool HasEnded(long id)
        {
            if (GetCountdown( id, out CountdownOperator cntOperator )) {
                return cntOperator.HasEnded();
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
            if (GetCountdown( id, out CountdownOperator cntOperator )) {
                return cntOperator is ScheduledCountdownOperator;
            }
            return false;
        }

        public static bool IsScheduled(ICountdown countdown)
        {
            return countdown is ScheduledCountdown;
        }

        /// <summary>
        /// Pauses/resumes time
        /// </summary>
        /// <param name="id">ID of affected time</param>
        /// <returns><b>True</b> if time has been found, otherwise <b>false</b></returns>
        public static bool Pause(long id)
        {
            if (GetCountdownNotEnded( id, out CountdownOperator cntOperator )) {
                cntOperator.Pause();
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
            if (GetCountdownNotEnded( id, out CountdownOperator cntOperator )) {
                cntOperator.Stop();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reset time times of all countdowns.
        /// </summary>
        public static void RestartAll()
        {
            RestartActive();
            RestartUnactive();
        }

        /// <summary>
        /// Reset time times of all not ended countdowns.
        /// </summary>
        public static void RestartActive()
        {
            foreach (CountdownOperator countdown in Instance.toCheckInNextFrame.Values) {
                countdown.Restart();
            }
        }

        /// <summary>
        /// Reset time times of all ended countdowns.
        /// </summary>
        public static void RestartUnactive()
        {
            foreach (CountdownOperator countdown in Instance.ended.Values) {
                countdown.Restart();
            }
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

        /*


         * */
        #endregion

        #region Private Methods
        // Main method
        private void Tick(float passedTime)
        {
            time += passedTime;
            isTicking = true;

            LinkedList<CountdownOperator> endInThisFrame = new LinkedList<CountdownOperator>();

            foreach (CountdownOperator countdown in toCheckInNextFrame.Values) {

                if (!countdown.Paused && countdown.ShouldEndTick()) {
                    endInThisFrame.AddLast( countdown );
                }
            }

            foreach (CountdownOperator countdown in endInThisFrame) {
                countdown.OnEndBehaviour();
                if (countdown.ShouldRestart() == false) {
                    toCheckInNextFrame.Remove( countdown.ID );
                    ended.Add( countdown.ID, countdown );
                } else {
                    countdown.RestartWhenShould();
                }
            }

            isTicking = false;
        }

        private static bool IsScheduled(CountdownOperator cntOperator)
        {
            return cntOperator is ScheduledCountdownOperator;
        }

        /// <summary>
        /// Gets time from singleton named Instance. If time with id doesn't exists it returns null.
        /// </summary>
        /// <param name="id">id of time to get</param>
        /// <returns>Countdown with given id</returns>
        private static bool GetCountdown(long id, out CountdownOperator cntOperator)
        {
            if (Instance.ended.TryGetValue( id, out cntOperator ) ||
                Instance.toCheckInNextFrame.TryGetValue( id, out cntOperator )) {
                return true;
            }
            return false;
        }

        private static bool GetCountdownNotEnded(long id, out CountdownOperator cntOperator)
        {
            if (Instance.ended.TryGetValue( id, out cntOperator )) {
                return true;
            }
            return false;
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
        public static int EndedCount => Instance.ended.Count;
        public static int CheckInNextFrameCount => Instance.toCheckInNextFrame.Count;
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


        internal class CountdownOperator : ICloneable
        {
            protected LinkedList<IObserver<ICountdown>> Observers { get; set; } = new LinkedList<IObserver<ICountdown>>();
            public ICountdown Countdown { get; set; }
            public Action<float> Action { get; set; }
            public long ID { get; protected set; }
            public float Remaing => Started ? ( Paused ? EndTime - PauseTime : EndTime - Instance.time ) : Time;
            public float Time { get; set; } = 0f;
            public float StartTime { get; set; } = 0f;
            public float EndTime { get; set; } = 0f;
            public float PauseTime { get; set; } = 0f;
            public float StopTime { get; set; } = 0f;
            public bool Started { get; set; } = false;
            public bool Paused { get; set; } = false;
            public bool Stopped { get; set; } = false;
            public bool Obsolete { get; set; } = false;
            public bool ForceRestart { get; set; } = false;

            protected CountdownOperator()
            { }

            public CountdownOperator(Countdown countdown, float time, Action<float> action, long ID)
            {
                Countdown = countdown;
                Time = time;
                Action = action;
                this.ID = ID;
            }

            public virtual void Restart()
            {
                Restart( Time );
            }

            public virtual void Restart(float time)
            {
                if (Started == false) {
                    Instance.ended.Remove( ID );
                    Instance.toCheckInNextFrame.Add( ID, this );
                }
                Time = time;
                Paused = false;
                Started = true;
                Stopped = false;
                StartTime = Instance.time;
                EndTime = Instance.time + Time;
                ForceRestart = true;
            }

            public virtual float Pause()
            {
                if (Started && !Paused) {
                    PauseTime = Instance.time;
                    Paused = true;
                    return Instance.time;
                }
                return float.MinValue;
            }

            public virtual float Unpause()
            {
                if (Started && Paused) {
                    Paused = false;
                    return Instance.time;
                }
                return float.MinValue;
            }

            public virtual float Stop()
            {
                if (Started) {
                    StopTime = Instance.time;
                    Stopped = true;
                    Paused = false;
                    Started = false;
                    ForceRestart = false;
                    // Jeżeli nie ticka to znaczy, że metoda została wywołana
                    // poza OnEndAction, musimy ręcznie usunąć countdown by nie
                    // został sprawdzony w następnej pętli
                    if (Instance.isTicking == false) {
                        Instance.toCheckInNextFrame.Remove( ID );
                        if (Instance.ended.ContainsKey( ID ) == false)
                            Instance.ended.Add( ID, this );
                    }
                    return StopTime;
                }
                return -1f;
            }

            public virtual void Destroy()
            {
                Instance.toCheckInNextFrame.Remove( ID );
                Instance.ended.Remove( ID );
                Stopped = false;
                Paused = false;
                Started = false;
                Time = -1;
                EndTime = -1;
                StartTime = -1;
                PauseTime = -1;
                StopTime = -1;
                Obsolete = true;
            }

            public virtual bool ShouldRestart()
            {
                return ForceRestart;
            }

            public virtual bool HasEnded()
            {
                return Stopped || EndTime <= Instance.time;
            }

            public virtual bool ShouldEndTick()
            {
                return EndTime <= Instance.time;
            }

            public virtual void OnEndBehaviour()
            {
                if (Action != null) {
                    Action.Invoke( Mathf.Abs( EndTime - Instance.time ) );
                }

                foreach (IObserver<ICountdown> observer in Observers) {
                    observer.Notify( Countdown );
                }
            }

            public virtual void RestartWhenShould()
            {
                ForceRestart = false;
            }

            public void Subscribe(IObserver<ICountdown> observer)
            {
                Observers.AddLast( observer );
            }

            public void Unsubscribe(IObserver<ICountdown> observer)
            {
                Observers.Remove( observer );
            }

            public virtual object Clone()
            {
                CountdownOperator clone = new CountdownOperator {
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Time = Time,
                    Stopped = Stopped,
                    Paused = Paused,
                    Started = Started,
                    StopTime = StopTime,
                    PauseTime = PauseTime,
                    Action = Action,
                    ID = NextID()
                };
                return clone;
            }
        }

        internal class ScheduledCountdownOperator : CountdownOperator
        {
            public int Repeats { get; set; }
            public int CurrentRepeat { get; set; }

            public ScheduledCountdownOperator(Countdown countdown, float time, int repeats, Action<float> action, long ID) : base( countdown, time, action, ID )
            {
                Repeats = repeats;
            }

            public override void Restart()
            {
                //Debug.Log( "Restart() Current repeat: " + CurrentRepeat );
                Restart( Time, Repeats );
            }

            public override void Restart(float time)
            {
               // Debug.Log( "Restart(float time) Current repeat: " + CurrentRepeat );
                Restart( time, Repeats );
            }

            public void Restart(float time, int repeats)
            {
                //Debug.Log( "Restart(float time, int repeats) Current repeat: " + CurrentRepeat );
                ChangeRepeats( repeats );
                base.Restart( time );
            }

            public override void RestartWhenShould()
            {
                ForceRestart = false;
                StartTime = Instance.time;
                EndTime = Instance.time + Time;
            }

            public override void OnEndBehaviour()
            {
                CurrentRepeat--;
                if (Repeats < 0 || CurrentRepeat > 0 ) {
                    ForceRestart = true;
                }
                base.OnEndBehaviour();
            }

            public override bool HasEnded()
            {
                return Stopped || !Started || CurrentRepeat == 0;
            }

            /*
            public override bool ShouldRestart()
            {
                if (Repeats < 0 || CurrentRepeat > 0) {
                    return true;
                }
                return base.ShouldRestart();
            }*/

            public override object Clone()
            {
                ScheduledCountdownOperator clone = base.Clone() as ScheduledCountdownOperator;
                clone.Repeats = Repeats;
                clone.CurrentRepeat = CurrentRepeat;
                return clone;
            }

            public void ChangeRepeats(int repeats)
            {
                CurrentRepeat = repeats;
                Repeats = repeats;
            }

            public void AddRepeat()
            {
                CurrentRepeat++;
                if (CurrentRepeat > Repeats) {
                    Repeats = CurrentRepeat;
                }
            }

            public void SubRepeat()
            {
                CurrentRepeat--;
            }
        }

        internal class Countdown : ICountdown
        {
            protected static readonly string OBSOLETE_EXCEPTION_MESSAGE =
                 "Countdown is Obsolete, it should be nulled or replaced by other value. " +
                 "Only method Clone() is allowed to use.";
            protected CountdownOperator @operator;

            protected Countdown()
            { }

            public static Countdown GetInstance(float time = 1f, Action<float> action = null)
            {
                if (Instance == null) {
                    throw new MissingEssentialGameObjectException( "Cannot create countdown without active TimerManager object on scene" );
                }
                if (time < 0)
                    throw new ArgumentException( "Countdown time cannot be less than 0!" );

                Countdown countdown = new Countdown();
                countdown.@operator = new CountdownOperator( countdown, time, action, NextID() );
                Instance.ended.Add( countdown.@operator.ID, countdown.@operator );
                return countdown;
            }

            public virtual void Restart()
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                @operator.Restart();
            }

            public virtual void Restart(float time)
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                @operator.Restart( time );
            }

            public virtual float Pause()
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                return @operator.Pause();
            }

            public float Unpause()
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                return @operator.Unpause();
            }

            public virtual float Stop()
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                return @operator.Stop();
            }

            public virtual bool HasEnded()
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                return @operator.HasEnded();
            }

            public virtual void Subscribe(IObserver<ICountdown> observer)
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                @operator.Subscribe( observer );
            }

            public virtual void Unsubscribe(IObserver<ICountdown> observer)
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                @operator.Unsubscribe( observer );
            }

            public void Destroy()
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                @operator.Destroy();
            }

            public void SetAction(Action<float> action)
            {
                @operator.Action = action;
            }

            public virtual object Clone()
            {
                CountdownOperator cloneOperator = @operator.Clone() as CountdownOperator;
                Countdown clone = new Countdown {
                    @operator = cloneOperator
                };
                cloneOperator.Countdown = clone;
                return clone;
            }

            public Action<float> Action => @operator.Action;
            public long ID => @operator.ID;
            public float Remaing => @operator.Remaing;
            public float Time => @operator.Time;
            public float StartTime => @operator.StopTime;
            public float EndTime => @operator.EndTime;
            public float PauseTime => @operator.PauseTime;
            public float StopTime => @operator.StopTime;
            public bool Started => @operator.Started;
            public bool Paused => @operator.Paused;
            public bool Stopped => @operator.Stopped;
            public bool Obsolete => @operator.Obsolete;
        }

        internal class ScheduledCountdown : Countdown
        {
            protected ScheduledCountdown() { }

            public static ScheduledCountdown GetInstance(float time = 1f, int repeats = 0, Action<float> action = null)
            {
                if (Instance == null) {
                    throw new MissingEssentialGameObjectException( "Cannot create time without active TimerManager object on scene" );
                }

                if (time < 0) {
                    throw new ArgumentException( "Countdown time cannot be less than 0!" );
                }

                ScheduledCountdown countdown = new ScheduledCountdown();
                countdown.@operator = new ScheduledCountdownOperator( countdown, time, repeats, action, NextID() );
                Instance.ended.Add( countdown.@operator.ID, countdown.@operator );
                return countdown;
            }

            public override void Restart()
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                ( @operator as ScheduledCountdownOperator ).Restart( Time, Repeats );
            }

            public override void Restart(float time)
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                ( @operator as ScheduledCountdownOperator ).Restart( time, Repeats );
            }

            public void Restart(float time, int repeats)
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                ( @operator as ScheduledCountdownOperator ).Restart( time, repeats );
            }

            /// <summary>
            /// Adds one repeat to current countdown
            /// </summary>
            public void AddRepeat()
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                ( @operator as ScheduledCountdownOperator ).AddRepeat();
            }

            /// <summary>
            /// Remove one repeat to current countdown
            /// </summary>
            public void SubRepeat()
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                ( @operator as ScheduledCountdownOperator ).SubRepeat();
            }

            /// <summary>
            /// Change number of repeats
            /// </summary>
            /// <param name="repeats"></param>
            public void ChangeRepeat(int repeats)
            {
                if (Obsolete)
                    throw new InvalidOperationException( OBSOLETE_EXCEPTION_MESSAGE );
                ( @operator as ScheduledCountdownOperator ).ChangeRepeats( repeats );
            }

            public override object Clone()
            {
                ScheduledCountdown clone = base.Clone() as ScheduledCountdown;
                ( clone.@operator as ScheduledCountdownOperator ).CurrentRepeat = CurrentRepeat;
                ( clone.@operator as ScheduledCountdownOperator ).Repeats = Repeats;
                return clone;
            }

            public int Repeats { get => ( @operator as ScheduledCountdownOperator ).Repeats; }
            public int CurrentRepeat { get => ( @operator as ScheduledCountdownOperator ).CurrentRepeat; }
        }
    }
}