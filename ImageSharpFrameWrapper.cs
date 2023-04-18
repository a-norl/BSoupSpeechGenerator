using System.Runtime.CompilerServices;
using FFMpegCore.Pipes;

namespace BSoupSpeechGenerator;

public class ImageSharpFrameWrapper<T> : IVideoFrame, IDisposable where T : unmanaged, IPixel<T>
{
    public int Width => Source.Width;

    public int Height => Source.Height;

    public string Format => "rgba";

    public Image<T> Source { get; private set; }

    public ImageSharpFrameWrapper(Image<T> source) {
        Source = source;
    }

    public void Dispose()
    {
        Source.Dispose();
    }

    public void Serialize(Stream stream) {
        byte[] pixelBytes = new byte[Source.Width * Source.Height * Unsafe.SizeOf<T>()];
        Source.CopyPixelDataTo(pixelBytes);
        stream.Write(pixelBytes, 0, pixelBytes.Length);
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token) {
        var pixelBytes = new byte[Source.Width * Source.Height * Unsafe.SizeOf<T>()];
        Source.CopyPixelDataTo(pixelBytes);
        await stream.WriteAsync(pixelBytes, 0, pixelBytes.Length);
    }
}