namespace QOATool;
class WavFile
{
    public int Channels;
    public int SampleRate;
    public short[] Samples;

    public static WavFile Read(string path)
    {
        using (var fs = File.OpenRead(path))
        using (var reader = new BinaryReader(fs))
        {
            // RIFF parsing
            if (new string(reader.ReadChars(4)) != "RIFF") throw new Exception("Not a RIFF file");
            reader.ReadInt32(); // File Size
            if (new string(reader.ReadChars(4)) != "WAVE") throw new Exception("Not a WAVE file");
                
            // Find fmt chunk
            while(true) 
            {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();
                if (chunkId == "fmt ")
                {
                    int formatTag = reader.ReadInt16(); // 1 = PCM
                    int channels = reader.ReadInt16();
                    int sampleRate = reader.ReadInt32();
                    reader.ReadInt32(); // Byte Rate
                    reader.ReadInt16(); // Block Align
                    int bitsPerSample = reader.ReadInt16();

                    if (formatTag != 1) throw new Exception("Only PCM WAV supported");
                    if (bitsPerSample != 16) throw new Exception("Only 16-bit WAV supported");

                    // Skip remaining fmt bytes if any
                    if (chunkSize > 16) reader.ReadBytes(chunkSize - 16);

                    // Find data chunk
                    while(true)
                    {
                        string dChunkId = new string(reader.ReadChars(4));
                        int dChunkSize = reader.ReadInt32();
                        if (dChunkId == "data")
                        {
                            int sampleCount = dChunkSize / 2; // 16-bit = 2 bytes
                            short[] data = new short[sampleCount];
                            for (int i = 0; i < sampleCount; i++)
                            {
                                data[i] = reader.ReadInt16();
                            }
                            return new WavFile { Channels = channels, SampleRate = sampleRate, Samples = data };
                        }
                        reader.ReadBytes(dChunkSize); // Skip other chunks
                    }
                }
                reader.ReadBytes(chunkSize);
            }
        }
    }

    public static void Write(string path, short[] samples, int channels, int sampleRate)
    {
        using (var fs = File.Create(path))
        using (var writer = new BinaryWriter(fs))
        {
            int dataSize = samples.Length * 2;
                
            writer.Write("RIFF"u8.ToArray());
            writer.Write(36 + dataSize);
            writer.Write("WAVE"u8.ToArray());
                
            writer.Write("fmt "u8.ToArray());
            writer.Write(16); // Chunk size
            writer.Write((short)1); // PCM
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * 2); // Byte rate
            writer.Write((short)(channels * 2)); // Block align
            writer.Write((short)16); // Bits per sample
                
            writer.Write("data"u8.ToArray());
            writer.Write(dataSize);
                
            foreach (var sample in samples)
            {
                writer.Write(sample);
            }
        }
    }
}
