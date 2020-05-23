using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Preprocess
{
    public static class ResizeProcessor
    {
        public static void Resize(Image<Rgb24> img)
        {
            img.Mutate(x => x.Resize(256, 256));
        }
    }
}