using System.Collections.Concurrent;

using GZipTest.Logger;

namespace GZipTest;

public abstract class AbstractFileBlocksProcessor
{
    private protected readonly BlockingCollection<DataBlock> readedBlocks;
    private protected readonly ObservedConcurrentDictionary<byte[]> dataBlocksForWriting;

    private protected readonly string inputFilePath;
    private protected readonly string outputFilePath;

    private protected readonly EventWaitHandle handle;

    private protected volatile int numberOfBlocksRead;

    private protected volatile int numberOfRecordedBlocks;

    private protected FileStream? outputFileStream;

    private protected ILogger logger;

    private protected readonly object locker;

    public AbstractFileBlocksProcessor(string inputFilePath, string outputFilePath, ILogger logger)
    {
        this.inputFilePath = inputFilePath;
        this.outputFilePath = outputFilePath;

        this.logger = logger;

        this.readedBlocks = new(Constants.QueueSize);
        this.dataBlocksForWriting = new();

        this.outputFileStream = default;

        this.handle = new AutoResetEvent(false);

        this.numberOfBlocksRead = 0;
        this.numberOfRecordedBlocks = 0;

        this.locker = new();

        this.dataBlocksForWriting.NextBlockIsInCollection += WritingBlock;
    }

    public abstract IAsyncEnumerable<DataBlock> GetCollectionOfBlocks();

    public abstract void ActionWithBlock(DataBlock block);

    public abstract bool WritingAction(int index);

    private void WritingBlock(int index)
    {
        if (this.WritingAction(index))
        {
            Interlocked.Increment(ref this.numberOfRecordedBlocks);
        }

        this.logger.WriteMessage($"{Constants.EventMessageFileBlockHasBeenWritten} {index}");

        if (this.readedBlocks.IsCompleted && this.numberOfBlocksRead == this.numberOfRecordedBlocks)
        {
            this.handle.Set();
        }
        else
        {
            this.dataBlocksForWriting.CheckingAvailabilityOfNecessaryBlock();
        }
    }

    private async Task ReadingBloсks()
    {
        await foreach (DataBlock block in GetCollectionOfBlocks())
        {
            this.readedBlocks.Add(block);
            this.logger.WriteMessage($"{Constants.EventMessageFileBlockHasBeenRead} {block.BlockIndex}");
            Interlocked.Increment(ref this.numberOfBlocksRead);
        }
        this.readedBlocks.CompleteAdding();
    }

    private void ProcessingBlocks()
    {
        Parallel.ForEach<DataBlock>(readedBlocks.GetConsumingEnumerable(), (block) =>
        {
            this.ActionWithBlock(block);
            this.logger.WriteMessage($"{Constants.EventMessageFileBlockHasBeenProcessed} {block.BlockIndex}");
            this.dataBlocksForWriting.CheckingAvailabilityOfNecessaryBlock();
        });
    }

    public async void Run(CancellationToken token)
    {
        this.outputFileStream = new FileStream(this.outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Write, Constants.BlockSize * 2, true);

        Task readingTask = this.ReadingBloсks();

        this.ProcessingBlocks();

        await readingTask;

        this.handle.WaitOne();

        this.outputFileStream.Close();

        if (token.IsCancellationRequested)
        {
            File.Delete(this.outputFilePath);
        }
    }
}