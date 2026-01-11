namespace QOATool;

class QOAStreamDecoder : QOADecoder
{
    private readonly Stream _stream;
    public QOAStreamDecoder(Stream stream) => _stream = stream;

    protected override int ReadByte() => _stream.ReadByte();

    protected override void SeekToByte(int position)
    {
        if (_stream.CanSeek)
            _stream.Seek(position, SeekOrigin.Begin);
    }
}
