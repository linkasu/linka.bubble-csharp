namespace Linka.Bubble
{
    internal sealed class BubbleSettings
    {
        public int Diameter { get; set; } = 44;
        public int Brightness { get; set; } = 65;
        public int Smoothness { get; set; } = 40;
        public bool Visible { get; set; } = true;

        public void Clamp()
        {
            Diameter = System.Math.Max(20, System.Math.Min(100, Diameter));
            Brightness = System.Math.Max(10, System.Math.Min(100, Brightness));
            Smoothness = System.Math.Max(0, System.Math.Min(100, Smoothness));
        }
    }
}
