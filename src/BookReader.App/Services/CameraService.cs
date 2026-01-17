using OpenCvSharp;
using System.Windows.Media.Imaging;
using System.IO;

namespace BookReader.App.Services;

public static class CameraService
{
    public static async Task<Mat?> CaptureAsync(int deviceIndex = 0)
    {
        return await Task.Run(() =>
        {
            using var cap = new VideoCapture(deviceIndex);
            if (!cap.IsOpened()) return null;
            using var frame = new Mat();
            cap.Read(frame);
            return frame.Empty() ? null : frame.Clone();
        });
    }
}
