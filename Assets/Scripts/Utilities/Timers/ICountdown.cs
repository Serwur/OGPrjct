using System;

namespace ColdCry.Utility.Time
{
    public interface ICountdown : IObservable<ICountdown>, IUniqueable<long>
    {
        void Start(); // startuje timer, wyłącza pauze
        void Restart();
        void Restart(float time); // restartuje timer z nowym nastawionym czasem
        void Pause();
        void Stop();
        bool HasEnded(); // czy koniec?
        void OnEnd(); // akcja po zakonczeniu
        bool ShouldRestart();

        Action<float> OnEndAction { get; set; }
        float Remaing { get; }
        float Time { get; }
        float StartTime { get; }
        float EndTime { get; }
        bool Paused { get; } // czy pauza
    }
}