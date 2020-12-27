using System;
using System.Collections.Generic;
using System.Threading;

namespace dug.Services
{
    public class PercentageAnimator : IPercentageAnimator, IDisposable {

        private const string _sequence = @"/-\|";
        private int _counter = 0;
        private readonly int _delay = 100;
        private bool _active;
        private readonly Thread _thread;
        private int _left, _top;
        private double _totalEvents;
        private double _count;
        private int _progressBarLength;
        private object _syncObject = 0;
        private string _customString;

        public PercentageAnimator (){
            _thread = new Thread(Spin);
        }

        public void EventHandler(string customString = null){
            lock(_syncObject){
                _count++;
                _customString = customString ?? " ";
            }
        }

        public void Start(string header, double totalEvents, int progressBarLength = 50)
        {
            Console.WriteLine(header);
            _totalEvents = totalEvents;
            _count = 0;
            _progressBarLength = progressBarLength;
            _left = Console.CursorLeft;
            _top = Console.CursorTop;
            
            _active = true;
            if (!_thread.IsAlive){
                _thread.Start();
            }
        }

        public void Stop()
        {
            _active = false;
            Draw(' ');
        }

        private void Spin()
        {
            while (_active)
            {
                Turn();
                Thread.Sleep(_delay);
            }
        }

        private void Draw(char c)
        {
            double progress;
            lock(_syncObject){
                progress = _count/_totalEvents;
            }
            Console.SetCursorPosition(_left, _top);
            Console.ForegroundColor = ConsoleColor.Green;
            
            Console.Write(c);
            Console.Write(" "+GetProgressBar(progress));
            Console.Write(_customString);
            Console.Write($" {(progress).ToString("P3")} ({_count}/{_totalEvents})");
            if(!_active){
                Console.Write(Environment.NewLine);
            }
        }

        private string GetProgressBar(double progress){
            string progressBar = "[";
            int progressBarFill = (int)(progress*_progressBarLength);
            for(int i = 0; i<progressBarFill; i++){
                progressBar += "=";
            }
            for(int i = 0; i<(_progressBarLength-progressBarFill); i++){
                progressBar += " ";
            }
            progressBar += "]";
            return progressBar;
        }

        private void Turn()
        {
            Draw(_sequence[_counter++ % _sequence.Length]);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
