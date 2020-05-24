using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Preprocess
{
    public static class ResizeProcessor
    {
        public static void Resize(Image<Rgb24> img, int size)
        {
            img.Mutate(x => x.Resize(size, size));
        }
    }
}