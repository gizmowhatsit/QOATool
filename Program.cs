namespace QOATool;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            PrintUsage();
            return;
        }

        string command = args[0].ToLower();
        string inputPath = args[1];

        bool mono = args.Any(a => a == "-m" || a == "--mono");

        try
        {
            switch (command)
            {
                case "wav2qoa":
                    if (args.Length < 3) { Console.WriteLine("Usage: wav2qoa <input.wav> <output.qoa> [-m]"); return; }
                    string outputPath = args[2];
                    ConvertWavToQoa(inputPath, outputPath, mono);
                    break;

                case "qoa2wav":
                    if (args.Length < 3) { Console.WriteLine("Usage: qoa2wav <input.qoa> <output.wav>"); return; }
                    ConvertQoaToWav(inputPath, args[2]);
                    break;

                default:
                    PrintUsage();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace); 
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("QOA CLI Tool");
        Console.WriteLine("Usage:");
        Console.WriteLine("  wav2qoa <input.wav> <output.qoa> [-m]  Convert WAV to QOA (optional -m for mono)");
        Console.WriteLine("  qoa2wav <input.qoa> <output.wav>       Convert QOA to WAV");
    }

    static void ConvertWavToQoa(string wavPath, string qoaPath, bool mixDownToMono)
    {
        Console.WriteLine($"Reading WAV: {wavPath}");
        var wav = WavFile.Read(wavPath);

        if (mixDownToMono && wav.Channels > 1)
        {
            Console.WriteLine($"Mixing down {wav.Channels} channels to Mono...");
            
            int originalChannels = wav.Channels;
            int totalFrames = wav.Samples.Length / originalChannels;
            short[] monoSamples = new short[totalFrames];

            for (int i = 0; i < totalFrames; i++)
            {
                int sum = 0;
                for (int c = 0; c < originalChannels; c++)
                {
                    sum += wav.Samples[i * originalChannels + c];
                }
                monoSamples[i] = (short)(sum / originalChannels);
            }

            wav.Samples = monoSamples;
            wav.Channels = 1;
        }

        Console.WriteLine($"Encoding QOA: {qoaPath} ({wav.SampleRate}Hz, {wav.Channels}ch, {wav.Samples.Length / wav.Channels} samples)");
            
        using (var fs = File.Create(qoaPath))
        {
            var encoder = new QOAStreamEncoder(fs);

            int totalSamples = wav.Samples.Length / wav.Channels;
                
            if (!encoder.WriteHeader(totalSamples, wav.Channels, wav.SampleRate))
                throw new Exception("Failed to write QOA header");
                
            int cursor = 0;
            int remaining = totalSamples;
            short[] buffer = wav.Samples;

            while (remaining > 0)
            {
                int chunkCount = Math.Min(remaining, QOABase.MaxFrameSamples);
                int bufferOffset = cursor * wav.Channels;
                    
                short[] frameBuffer = new short[chunkCount * wav.Channels];
                Array.Copy(buffer, bufferOffset, frameBuffer, 0, frameBuffer.Length);

                if (!encoder.WriteFrame(frameBuffer, chunkCount))
                {
                    throw new Exception("Failed to write QOA frame");
                }

                cursor += chunkCount;
                remaining -= chunkCount;
            }
        }
        Console.WriteLine("Conversion complete.");
    }

    static void ConvertQoaToWav(string qoaPath, string wavPath)
    {
        Console.WriteLine($"Reading QOA: {qoaPath}");
        using (var fs = File.OpenRead(qoaPath))
        {
            var decoder = new QOAStreamDecoder(fs);
            if (!decoder.ReadHeader())
            {
                throw new Exception("Invalid QOA header");
            }

            int totalSamples = decoder.GetTotalSamples();
            int channels = decoder.GetChannels();
            int sampleRate = decoder.GetSampleRate();

            Console.WriteLine($"Decoding: {sampleRate}Hz, {channels}ch, {totalSamples} samples");

            List<short> allSamples = new List<short>(totalSamples * channels);
            short[] frameBuffer = new short[QOABase.MaxFrameSamples * channels];

            while (!decoder.IsEnd())
            {
                int samplesDecoded = decoder.ReadFrame(frameBuffer);
                if (samplesDecoded < 0) break;

                for (int i = 0; i < samplesDecoded * channels; i++)
                {
                    allSamples.Add(frameBuffer[i]);
                }
            }

            Console.WriteLine($"Writing WAV: {wavPath}");
            WavFile.Write(wavPath, allSamples.ToArray(), channels, sampleRate);
        }
        Console.WriteLine("Conversion complete.");
    }
}
