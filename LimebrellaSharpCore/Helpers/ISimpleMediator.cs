// v2024-08-03 21:16:48

namespace LimebrellaSharpCore.Helpers;

public interface ISimpleMediator
{
    /// <summary>
    /// Specifies identifiers to indicate the return value of a dialog.
    /// </summary>
    public enum DialogAnswer
    {
        None,
        Ok,
        Cancel,
        Abort,
        Retry,
        Ignore,
        Yes,
        No,
        TryAgain,
        Continue
    }

    /// <summary>
    /// Specifies dialog options.
    /// </summary>
    public enum QuestionOptions
    {
        OkCancel,
        AbortRetryIgnore,
        YesNoCancel,
        YesNo,
        RetryCancel,
        CancelTryContinue
    }

    /// <summary>
    /// Specifies type of dialog.
    /// </summary>
    public enum DialogType
    {
        None,
        Question,
        Exclamation,
        Error,
        Warning,
        Information
    }

    /// <summary>
    /// Ask the user a simple question and get an answer.
    /// </summary>
    /// <param name="question"></param>
    /// <param name="caption"></param>
    /// <param name="questionOptions"></param>
    /// <param name="dialogType"></param>
    DialogAnswer Ask(string question, string caption, QuestionOptions questionOptions, DialogType dialogType);

    /// <summary>
    /// Send a message to the user.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="caption"></param>
    /// <param name="dialogType"></param>
    void Inform(string info, string caption, DialogType dialogType);
}