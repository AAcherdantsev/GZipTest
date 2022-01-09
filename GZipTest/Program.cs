using GZipTest;
using GZipTest.Logger;

Engine engine = new(new Logger());
ValidationStatus validationResult = engine.Validation(args);

if (validationResult == ValidationStatus.Success)
{
    engine.ProcessorInitialization(args);

    CancellationTokenSource canceller = new();

    engine.Run(canceller.Token);

    return 0;
}
else
{
    engine.ShowValidationErrorMessage(validationResult);
    return 1;
}

//Console.CancelKeyPress

//if (Console.ReadKey().Key == Constants.ButtonToCancel)
//{
//    canceller.Cancel();
//    return 1;
//}
//
//return 0;