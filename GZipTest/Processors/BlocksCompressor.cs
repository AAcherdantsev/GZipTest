using System.IO.Compression;

using GZipTest.Logger;

namespace GZipTest.Processors;

public class BlocksCompressor : AbstractFileBlocksProcessor
{
    public BlocksCompressor(string inputFilePath, string outputFilePath, ILogger logger) :
        base(inputFilePath, outputFilePath, logger)
    {
    }

    public override void ActionWithBlock(DataBlock block)
    {
        using MemoryStream stream = new();
        using (GZipStream gZipStream = new(stream, CompressionMode.Compress))
        {
            gZipStream.Write(block.data, 0, block.Size);
        }
        dataBlocksForWriting.TryAdd(block.BlockIndex, stream.ToArray());
    }

    public override async IAsyncEnumerable<DataBlock> GetCollectionOfBlocks()
    {
        FileStream inputFileStream = new(this.inputFilePath, FileMode.Open);

        int index = 0;
        byte[] bytes = new byte[Constants.BlockSize];

        while (await inputFileStream.ReadAsync(bytes.AsMemory(0, Constants.BlockSize)) != 0)
        {
            yield return new DataBlock(index++, bytes);
            bytes = new byte[Constants.BlockSize];
        }

        inputFileStream.Close();
        yield break;
    }

    public override bool WritingAction(int index)
    {
        lock (this.locker)
        {
            if (this.dataBlocksForWriting.TryRemove(index, out byte[]? bytes))
            {
                byte[] sizeBytes = BitConverter.GetBytes(bytes.Length);

                this.outputFileStream.Write(sizeBytes);
                this.outputFileStream.Write(bytes);

                return true;
            }

            return false;
        }
    }
}