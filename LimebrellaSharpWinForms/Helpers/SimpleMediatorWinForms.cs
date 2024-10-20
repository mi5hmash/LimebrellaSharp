// v2024-08-06 23:50:49

using LimebrellaSharpCore.Helpers;
using static LimebrellaSharpCore.Helpers.ISimpleMediator;

namespace LimebrellaSharpWinforms.Helpers;

public class SimpleMediatorWinForms : ISimpleMediator
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public SimpleMediatorWinForms() { }

    /// <summary>
    /// Translates <see cref="ISimpleMediator.QuestionOptions"/> to <see cref="MessageBoxButtons"/>
    /// </summary>
    /// <param name="questionOptions"></param>
    /// <returns></returns>
    private static MessageBoxButtons GetDialogOpt(QuestionOptions questionOptions)
    {
        return questionOptions switch
        {
            QuestionOptions.OkCancel => MessageBoxButtons.OKCancel,
            QuestionOptions.AbortRetryIgnore => MessageBoxButtons.AbortRetryIgnore,
            QuestionOptions.YesNoCancel => MessageBoxButtons.YesNoCancel,
            QuestionOptions.YesNo => MessageBoxButtons.YesNo,
            QuestionOptions.RetryCancel => MessageBoxButtons.RetryCancel,
            QuestionOptions.CancelTryContinue => MessageBoxButtons.CancelTryContinue,
            _ => MessageBoxButtons.OKCancel
        };
    }

    /// <summary>
    /// Translates <see cref="ISimpleMediator.DialogType"/> to <see cref="MessageBoxIcon"/>
    /// </summary>
    /// <param name="dialogType"></param>
    /// <returns></returns>
    private static MessageBoxIcon GetDialogType(DialogType dialogType)
    {
        return dialogType switch
        {
            DialogType.None => MessageBoxIcon.None,
            DialogType.Question => MessageBoxIcon.Question,
            DialogType.Exclamation => MessageBoxIcon.Exclamation,
            DialogType.Error => MessageBoxIcon.Error,
            DialogType.Warning => MessageBoxIcon.Warning,
            DialogType.Information => MessageBoxIcon.Information,
            _ => MessageBoxIcon.None
        };
    }

    /// <summary>
    /// Ask the user a simple question and get an answer.
    /// </summary>
    /// <param name="question"></param>
    /// <param name="caption"></param>
    /// <param name="questionOptions"></param>
    /// <param name="dialogType"></param>
    /// <returns></returns>
    public DialogAnswer Ask(string question, string caption, QuestionOptions questionOptions, DialogType dialogType)
    {
        var dlgOpt = GetDialogOpt(questionOptions);
        var dlgType = GetDialogType(dialogType);

        var dialogResult = MessageBox.Show(question, caption, dlgOpt, dlgType);

        return dialogResult switch
        {
            DialogResult.None => DialogAnswer.None,
            DialogResult.OK => DialogAnswer.Ok,
            DialogResult.Cancel => DialogAnswer.Cancel,
            DialogResult.Abort => DialogAnswer.Abort,
            DialogResult.Retry => DialogAnswer.Retry,
            DialogResult.Ignore => DialogAnswer.Ignore,
            DialogResult.Yes => DialogAnswer.Yes,
            DialogResult.No => DialogAnswer.No,
            DialogResult.TryAgain => DialogAnswer.TryAgain,
            DialogResult.Continue => DialogAnswer.Continue,
            _ => DialogAnswer.None
        };
    }

    /// <summary>
    /// Send a message to the user.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="caption"></param>
    /// <param name="dialogType"></param>
    public void Inform(string info, string caption, DialogType dialogType)
    {
        var dlgType = GetDialogType(dialogType);
        MessageBox.Show(info, caption, MessageBoxButtons.OK, dlgType);
    }
}