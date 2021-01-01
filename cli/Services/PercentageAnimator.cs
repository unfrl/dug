using System;
using System.Diagnostics;
using System.Threading;
using Spectre.Console;

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
        private Stopwatch _stopwatch = new Stopwatch();
        private bool _trueColor = AnsiConsole.Capabilities.Supports(ColorSystem.TrueColor);

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
            if(Config.Verbose){
                Console.WriteLine("Progress Animation disabled during verbose output");
                return;
            }
            Console.WriteLine(header);
            _totalEvents = totalEvents;
            _count = 0;
            _progressBarLength = progressBarLength;
            _left = Console.CursorLeft;
            _top = Console.CursorTop;
            
            _active = true;
            if (!_thread.IsAlive){
                _stopwatch.Restart();
                _thread.Start();
            }
        }

        public void StopIfRunning()
        {
            if(!_active || Config.Verbose){
                return;
            }
            _active = false;
            _customString = " ";
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
            TimeSpan elapsed;
            lock(_syncObject){
                progress = _count/_totalEvents;
                elapsed= _stopwatch.Elapsed;
            }
            Console.SetCursorPosition(_left, _top);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(c);
            if(_trueColor){
                Console.ForegroundColor = ConsoleColor.White;
                var progressColor = Interpolate(Color.Red, Color.Green, progress);
                AnsiConsole.Background = progressColor;
                AnsiConsole.Write(GetProgressBar(progress));
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else{
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(GetProgressBar(progress));
            }
            Console.Write(_customString);
            Console.Write($" {(progress).ToString("P3")} ({_count}/{_totalEvents}) ({elapsed.ToString("mm\\:ss\\:fff")})");
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
            StopIfRunning();
        }

        private Color Interpolate(Color color1, Color color2, double fraction)
        {
            double r = InterpolateDoubles(color1.R, color2.R, fraction);
            double g = InterpolateDoubles(color1.G, color2.G, fraction);
            double b = InterpolateDoubles(color1.B, color2.B, fraction);
            return new Color(
                Convert.ToByte(r),
                Convert.ToByte(g),
                Convert.ToByte(b));
        }

        private double InterpolateDoubles(double d1, double d2, double fraction)
        {
            return d1 + (d2 - d1) * fraction;
        }
    }
}
