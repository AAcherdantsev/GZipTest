namespace GZipTest;

public class DataBlock
{
    public int BlockIndex { get; private set; }
    public int Size => data.Length;

    public byte[] data;
    public DataBlock(int blockIndex, byte[] data)
    {
        this.BlockIndex = blockIndex;
        this.data = data;
    }
}