using System.Diagnostics;

namespace Linka.Bubble
{
    internal sealed class GazeDisplayState
    {
        private readonly GazeFilter _filter = new GazeFilter();
        private long _receivedAt;
        private bool _valid;
        private double _x;
        private double _y;

        public void Reset()
        {
            _filter.Reset();
            _valid = false;
            _receivedAt = 0;
        }

        public void Record(double x, double y, double width, double height, long receivedAt, double smoothness)
        {
            var elapsed = _receivedAt == 0 ? 0 : (receivedAt - _receivedAt) / (double)Stopwatch.Frequency;
            _receivedAt = receivedAt;
            _valid = _filter.TryUpdate(x, y, width, height, elapsed, smoothness, out _x, out _y);
            if (!_valid) _filter.Reset();
        }

        public double AgeSeconds(long now, long startedAt) =>
            (now - (_receivedAt == 0 ? startedAt : _receivedAt)) / (double)Stopwatch.Frequency;

        public bool TryGetPosition(long now, out double x, out double y)
        {
            x = _x;
            y = _y;
            if (_valid && AgeSeconds(now, now) <= 0.3) return true;
            _valid = false;
            _filter.Reset();
            return false;
        }

        public bool HasSamples => _receivedAt != 0;
    }
}
