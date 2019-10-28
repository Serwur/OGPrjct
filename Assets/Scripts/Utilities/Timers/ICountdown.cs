using System;

namespace ColdCry.Utility.Time
{
    public interface ICountdown : IObservable<ICountdown>, ICloneable
    {
        void Restart();
        void Restart(float time); // restartuje timer z nowym nastawionym czasem
        float Pause();
        float Unpause();
        float Stop();
        bool HasEnded(); // czy koniec?
        void Destroy();
        void SetAction(Action<float> action);

        Action<float> Action { get; }
        long ID { get; }
        float Remaing { get; }
        float Time { get; }
        float StartTime { get; }
        float EndTime { get; }
        float PauseTime { get; }
        float StopTime { get; }
        bool Started { get; }
        bool Paused { get; } // czy pauza
        bool Stopped { get; }
        bool Obsolete { get; }
    }
}