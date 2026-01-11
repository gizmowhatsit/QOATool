namespace QOATool;

class QOAStreamEncoder : QOAEncoder
{
    private readonly Stream _stream;
    public QOAStreamEncoder(Stream stream) => _stream = stream;

    protected override bool WriteLong(long l)
    {
        // QOA is Big Endian. 
        // We manually shift bytes to ensure architecture independence.
        _stream.WriteByte((byte)(l >> 56));
        _stream.WriteByte((byte)(l >> 48));
        _stream.WriteByte((byte)(l >> 40));
        _stream.WriteByte((byte)(l >> 32));
        _stream.WriteByte((byte)(l >> 24));
        _stream.WriteByte((byte)(l >> 16));
        _stream.WriteByte((byte)(l >> 8));
        _stream.WriteByte((byte)l);
        return true;
    }
}
