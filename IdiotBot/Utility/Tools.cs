using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdiotBot.Utility;

public static class Tools
{
    public static async Task<Bitmap> GetBitmapFromUrl(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            byte[] imageBytes = await client.GetByteArrayAsync(url);

            using (MemoryStream ms = new MemoryStream(imageBytes))
                return new Bitmap(ms);
        }
    }

    public static Color GetAverageColor(Bitmap bmp)
    {
        long r = 0, g = 0, b = 0;
        int pixelCount = 0;

        for (int x = 0; x < bmp.Width; x++)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                Color pixel = bmp.GetPixel(x, y);
                r += pixel.R;
                g += pixel.G;
                b += pixel.B;
                pixelCount++;
            }
        }

        return Color.FromArgb((int)(r / pixelCount), (int)(g / pixelCount), (int)(b / pixelCount));
    }

}
