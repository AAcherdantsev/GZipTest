using System.IO.Compression;

using GZipTest.Logger;

namespace GZipTest.Processors;

public class BlocksDecompressor : AbstractFileBlocksProcessor
{
    public BlocksDecompressor(string inputFilePath, string outputFilePath, ILogger logger) :
        base(inputFilePath, outputFilePath, logger)
    {
    }

    public override void ActionWithBlock(DataBlock block)
    {
        using MemoryStream stream = new();
        using (GZipStream gZipStream = new(new MemoryStream(block.data), CompressionMode.Decompress))
        {
            gZipStream.CopyTo(stream);
        }
        dataBlocksForWriting.TryAdd(block.BlockIndex, stream.ToArray());
    }

    public override async IAsyncEnumerable<DataBlock> GetCollectionOfBlocksAsync()
    {
        FileStream inputFileStream = new(this.inputFilePath, FileMode.Open);

        int index = 0;

        byte[] bytes = new byte[sizeof(int)];
        int amountBytes = inputFileStream.Read(bytes);

        while (amountBytes == bytes.Length)
        {
            int blockLen = BitConverter.ToInt32(bytes);
            bytes = new byte[blockLen];

            await inputFileStream.ReadAsync(bytes.AsMemory(0, blockLen));

            yield return new DataBlock(index++, bytes);

            bytes = new byte[sizeof(int)];
            amountBytes = inputFileStream.Read(bytes);
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
                this.outputFileStream.Write(bytes);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}