using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.IO;
using System.Windows.Media.Imaging;

namespace BookReader.App.Services;

public static class ImageService
{
    public static async Task<(string coverPath, BitmapSource thumbnail)> SaveCoverAsync(Guid bookId, Mat mat)
    {
        var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BookReader", "covers");
        Directory.CreateDirectory(baseDir);
        var coverPath = Path.Combine(baseDir, $"{bookId}.jpg");

        // Uložit JPEG
        Cv2.ImWrite(coverPath, mat, new[] { (int)ImwriteFlags.JpegQuality, 90 });

        // Thumbnail (delší hrana 400 px)
        var scale = 400.0 / Math.Max(mat.Width, mat.Height);
        using var resized = new Mat();
        Cv2.Resize(mat, resized, new OpenCvSharp.Size(mat.Width * scale, mat.Height * scale));

        // Převod na BitmapSource
        var thumb = ToBitmapSource(resized);
        return (coverPath, thumb);
    }

    public static BitmapSource ToBitmapSource(Mat mat)
    {
        var image = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(mat);
        image.Freeze();
        return image;
    }
}
