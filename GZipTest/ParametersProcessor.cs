using System.IO.Compression;
using System.Security;

using GZipTest.Logger;

namespace GZipTest;

public enum ValidationStatus : byte
{
    Success,
    InvalidNumberOfArguments,
    UnknownMode,
    SourceFileNotFound,
    SourceFileAndOutputFileAreSame,
    OutputFileAlreadyExists,
    ErrorAccessingSourceFile,
    PathNotFound,
    UnknownError
}

internal sealed class ParametersProcessor
{
    private readonly ILogger logger;

    public ParametersProcessor(ILogger logger)
    {
        this.logger = logger;
    }

    public ValidationStatus Validate(string[] args)
    {
        if (args.Length != Constants.NumberOfArguments)
        {
            return ValidationStatus.InvalidNumberOfArguments;
        }

        if (args[Constants.ModeIndexInParameters] is not (Constants.KeywordForCompression or Constants.KeywordForDecompression))
        {
            return ValidationStatus.UnknownMode;
        }

        string sourceFilePath = Path.GetFullPath(args[Constants.SourceFilePathIndexInParameters]);
        string outputFilePath = Path.GetFullPath(args[Constants.OutputFilePathIndexInParameters]);

        if (!File.Exists(sourceFilePath))
        {
            return ValidationStatus.SourceFileNotFound;
        }

        if (File.Exists(outputFilePath))
        {
            return ValidationStatus.OutputFileAlreadyExists;
        }

        if (sourceFilePath == outputFilePath)
        {
            return ValidationStatus.SourceFileAndOutputFileAreSame;
        }

        ValidationStatus validationOutputFile = this.CheckFileByPath(outputFilePath, FileMode.Create);

        if (validationOutputFile != ValidationStatus.Success)
        {
            return validationOutputFile;
        }

        File.Delete(outputFilePath);

        return this.CheckFileByPath(sourceFilePath, FileMode.Open);
    }

    private ValidationStatus CheckFileByPath(string filePath, FileMode mode)
    {
        try
        {
            using (File.Open(Path.GetFullPath(filePath), mode))
            {
                return ValidationStatus.Success;
            }
        }

        catch (Exception ex)
        {
            this.logger.WriteError(ex.Message);

            return ex switch
            {
                FileNotFoundException or DirectoryNotFoundException => ValidationStatus.PathNotFound,

                IOException => ValidationStatus.OutputFileAlreadyExists,

                SecurityException => ValidationStatus.ErrorAccessingSourceFile,

                _ => ValidationStatus.UnknownError,
            };
        }
    }

    public Tuple<CompressionMode, string, string> GetParameters(string[] args)
    {
        CompressionMode mode = args[Constants.ModeIndexInParameters] == Constants.KeywordForCompression ?
            CompressionMode.Compress : CompressionMode.Decompress;

        return Tuple.Create(mode, args[Constants.SourceFilePathIndexInParameters], args[Constants.OutputFilePathIndexInParameters]);
    }

    public void ShowValidationErrorMessage(ValidationStatus status)
    {
        switch (status)
        {
            case ValidationStatus.InvalidNumberOfArguments:
                this.logger.WriteError($"{Constants.ErrorMessageInvalidNumberOfArguments}. {Constants.MessageWithSyntaxHint}");
                break;

            case ValidationStatus.UnknownMode:
                this.logger.WriteError($"{Constants.ErrorMessageUnknownMode}. {Constants.MessageWithSyntaxHint}");
                break;

            case ValidationStatus.SourceFileNotFound:
                this.logger.WriteError(Constants.ErrorMessageSourceFileNotFound);
                break;

            case ValidationStatus.SourceFileAndOutputFileAreSame:
                this.logger.WriteError(Constants.ErrorMessageSourceFileAndOutputFileAreSame);
                break;

            case ValidationStatus.OutputFileAlreadyExists:
                this.logger.WriteError(Constants.ErrorMessageOutputFileAlreadyExists);
                break;

            case ValidationStatus.ErrorAccessingSourceFile:
                this.logger.WriteError(Constants.ErrorMessageErrorAccessingSourceFile);
                break;

            case ValidationStatus.PathNotFound:
                this.logger.WriteError(Constants.ErrorMessagePathNotFound);
                break;

            case ValidationStatus.UnknownError:
                this.logger.WriteError(Constants.ErrorMessageUnknownError);
                break;
        }
    }
}