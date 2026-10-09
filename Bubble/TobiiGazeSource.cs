using System;
using System.Diagnostics;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

namespace Linka.Bubble
{
    internal sealed class TobiiGazeSource : IDisposable
    {
        private readonly object _sync = new object();
        private Host _host;
        private Action _unsubscribe;
        private double _x;
        private double _y;
        private long _receivedAt;
        private long _sequence;

        public void Start()
        {
            Stop();
            var host = new Host();
            try
            {
                host.EnableConnection();
                var stream = host.Streams.CreateGazePointDataStream();
                stream.Next += OnGaze;
                _host = host;
                _unsubscribe = () => stream.Next -= OnGaze;
            }
            catch
            {
                host.Dispose();
                throw;
            }
        }

        private void OnGaze(object sender, StreamData<GazePointData> sample)
        {
            var data = sample.Data;
            lock (_sync)
            {
                _x = data.X;
                _y = data.Y;
                _receivedAt = Stopwatch.GetTimestamp();
                _sequence++;
            }
        }

        public bool TryGetLatest(out double x, out double y, out long receivedAt, out long sequence)
        {
            lock (_sync)
            {
                x = _x;
                y = _y;
                receivedAt = _receivedAt;
                sequence = _sequence;
                return _sequence != 0;
            }
        }

        public void Stop()
        {
            var unsubscribe = _unsubscribe;
            var host = _host;
            _unsubscribe = null;
            _host = null;
            lock (_sync)
            {
                _sequence = 0;
                _receivedAt = 0;
            }
            try
            {
                unsubscribe?.Invoke();
            }
            finally
            {
                host?.Dispose();
            }
        }

        public void Dispose() => Stop();
    }
}
