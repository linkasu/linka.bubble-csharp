using System;
using System.Diagnostics;
using Linka.Bubble;

internal static class Program
{
    private static void Main()
    {
        var filter = new GazeFilter();
        Assert(filter.TryUpdate(100, 100, 1920, 1080, 0, 0.5, out var x, out var y) && x == 100 && y == 100,
            "first point must appear immediately");
        Assert(filter.TryUpdate(300, 100, 1920, 1080, 0.03, 0.5, out x, out y) && x > 100 && x < 300,
            "smoothing must move towards the new point");
        filter.Reset();
        Assert(filter.TryUpdate(300, 100, 1920, 1080, 0, 0.5, out x, out y) && x == 300,
            "recovered gaze must not travel from the previous position");
        Assert(!filter.TryUpdate(double.NaN, 1, 1920, 1080, 0, 0, out x, out y), "NaN must be rejected");
        Assert(!filter.TryUpdate(1920, 1, 1920, 1080, 0, 0, out x, out y), "off-screen point must be rejected");

        var display = new GazeDisplayState();
        var start = Stopwatch.GetTimestamp();
        display.Record(100, 100, 1920, 1080, start, 0.5);
        Assert(display.TryGetPosition(start, out x, out y), "fresh sample must be visible");
        Assert(!display.TryGetPosition(start + Stopwatch.Frequency / 2, out x, out y),
            "stale sample must disappear");
        display.Record(300, 100, 1920, 1080, start + Stopwatch.Frequency, 0.5);
        Assert(display.TryGetPosition(start + Stopwatch.Frequency, out x, out y) && x == 300,
            "new sample after loss must not interpolate from stale gaze");
        display.Record(double.NaN, 0, 1920, 1080, start + 2 * Stopwatch.Frequency, 0.5);
        Assert(!display.TryGetPosition(start + 2 * Stopwatch.Frequency, out x, out y),
            "explicitly invalid sample must disappear immediately");

        var settings = new BubbleSettings { Diameter = -10, Brightness = 999, Smoothness = 200 };
        settings.Clamp();
        Assert(settings.Diameter == 20 && settings.Brightness == 100 && settings.Smoothness == 100,
            "stored settings must be clamped");
        Console.WriteLine("Bubble checks passed.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
