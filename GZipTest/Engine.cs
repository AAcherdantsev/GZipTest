using System.IO.Compression;

using GZipTest.Logger;
using GZipTest.Processors;

namespace GZipTest;

public class Engine
{
    private AbstractFileBlocksProcessor? processor;

    private readonly ParametersProcessor parametersProcessor;

    private readonly ILogger logger;

    public Engine(ILogger logger)
    {
        this.logger = logger;
        this.parametersProcessor = new(logger);
    }

    public ValidationStatus Validation(string[] args)
    {
        return parametersProcessor.Validate(args);
    }

    public void ShowValidationErrorMessage(ValidationStatus status)
    {
        parametersProcessor.ShowValidationErrorMessage(status);
    }

    public void ProcessorInitialization(string[] args)
    {
        Tuple<CompressionMode, string, string> parameters = parametersProcessor.GetParameters(args);

        this.processor = parameters.Item1 is CompressionMode.Compress ?
            new BlocksCompressor(parameters.Item2, parameters.Item3, this.logger) :
            new BlocksDecompressor(parameters.Item2, parameters.Item3, this.logger);
    }

    public void Run(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(this.processor);

        this.processor.Run(token);
    }
}