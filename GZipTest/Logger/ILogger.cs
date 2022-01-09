namespace GZipTest.Logger;

public interface ILogger
{
    void WriteMessage(string message);

    void WriteError(string message);
}