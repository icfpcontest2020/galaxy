using System.Collections.Generic;
using System.Linq;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CosmicMachine.VisualLang
{
    public class ImageRendered
    {
        private readonly int margin;
        //public Rgba32 BackgroundColor = Rgba32.Black;
        //public Rgba32 ForegroundColor = Rgba32.Yellow;
        //public Rgba32 BorderColor = Rgba32.DarkOrange;
        public Gray8 BackgroundColor = new Gray8(0);
        public Gray8 ForegroundColor = new Gray8(255);
        public Gray8 BorderColor = new Gray8(192);

        public ImageRendered(int scale = 1, int margin = 1)
        {
            this.margin = margin;
            Scale = scale;
        }

        public int Scale { get; }

        public Image<Gray8> FromPoints(List<Vec> ps)
        {
            var width = ps.Max(p => p.X) + margin;
            var height = ps.Max(p => p.Y) + margin;
            var config = new Configuration();
            var image = new Image<Gray8>(config, width * Scale, height * Scale, BackgroundColor);
            foreach (var vec in ps)
            {
                if (Scale < 4)
                    for (int x = 0; x < Scale; x++)
                        for (int y = 0; y < Scale; y++)
                            image[vec.X * Scale + x, vec.Y * Scale + y] = ForegroundColor;
                else
                    for (int x = 0; x < Scale; x++)
                        for (int y = 0; y < Scale; y++)
                            image[vec.X * Scale + x, vec.Y * Scale + y] = x == Scale-1 || y == Scale-1 ? BorderColor : ForegroundColor;
            }
            return image;
        }
    }
}