namespace GZipTest
{
    public static class Constants
    {
        #region CommandLineArgumentConstants

        public const byte NumberOfArguments = 3;

        public const byte ModeIndexInParameters = 0;
        public const byte SourceFilePathIndexInParameters = 1;
        public const byte OutputFilePathIndexInParameters = 2;

        public const string KeywordForCompression = "compress";
        public const string KeywordForDecompression = "decompress";

        #endregion

        #region ErrorMessages

        public const string ErrorMessageInvalidNumberOfArguments = "Invalid number of arguments";
        public const string ErrorMessageUnknownMode = "Unknown mode";
        public const string ErrorMessageSourceFileNotFound = "Source file not found";
        public const string ErrorMessageSourceFileAndOutputFileAreSame = "The source file and the output file are the same";
        public const string ErrorMessageOutputFileAlreadyExists = "The output file already exists";
        public const string ErrorMessagePathNotFound = "Path not found";
        public const string ErrorMessageErrorAccessingSourceFile = "Error accessing the source file";
        public const string ErrorMessageUnknownError = "An unknown error has occurred";

        #endregion

        #region EventMessages

        public const string EventMessageFileBlockHasBeenRead = "The file block has been read";
        public const string EventMessageFileBlockHasBeenWritten = "The file block has been written";
        public const string EventMessageFileBlockHasBeenProcessed = "File block has been processed";

        public const string EventMessageOperationWasCanceledByUser = "The operation was canceled by the user";

        #endregion

        #region Other

        public const ConsoleKey ButtonToCancel = ConsoleKey.Escape;

        public const int BlockSize = 2 * 1024;
        public const int QueueSize = 31;

        public const string Error = "ERROR";


        public const string MessageWithSyntaxHint = $"Please use the syntax: GZipTest.exe [{KeywordForCompression} | {KeywordForDecompression}] [source_file_path] [source_file_path]";

        #endregion
    }
}
