namespace LimebrellaSharpCore.Helpers;

public class ProgressReporter(IProgress<string> text, IProgress<int> value)
{
    /// <summary>
    /// Reports message and progress.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="progress"></param>
    public void Report(string message, int progress)
    {
        text.Report(message);
        value.Report(progress);
    }

    /// <summary>
    /// Reports message.
    /// </summary>
    /// <param name="message"></param>
    public void Report(string message) 
        => text.Report(message);

    /// <summary>
    /// Reports progress.
    /// </summary>
    /// <param name="progress"></param>
    public void Report(int progress)
        => value.Report(progress);
}