namespace LimebrellaSharpCore.Models;

public class BoolResult
{
    public bool Result { get; set; }

    public string Description { get; set; }

    public BoolResult(bool result, string description = "")
    {
        Result = result;
        Description = description;
    }
}