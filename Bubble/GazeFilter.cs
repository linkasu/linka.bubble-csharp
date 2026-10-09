using System;

namespace Linka.Bubble
{
    internal sealed class GazeFilter
    {
        private double _x;
        private double _y;
        private bool _initialized;

        public void Reset() => _initialized = false;

        public bool TryUpdate(double x, double y, double width, double height, double elapsedSeconds,
            double smoothness, out double filteredX, out double filteredY)
        {
            filteredX = filteredY = 0;
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y) ||
                x < 0 || y < 0 || x >= width || y >= height)
                return false;

            if (!_initialized)
            {
                _x = x;
                _y = y;
                _initialized = true;
            }
            else
            {
                // Time-based smoothing is independent of tracker and display refresh rates.
                var timeConstant = 0.005 + Math.Max(0, Math.Min(1, smoothness)) * 0.20;
                var fraction = 1 - Math.Exp(-Math.Max(0, elapsedSeconds) / timeConstant);
                _x += fraction * (x - _x);
                _y += fraction * (y - _y);
            }

            filteredX = _x;
            filteredY = _y;
            return true;
        }
    }
}
