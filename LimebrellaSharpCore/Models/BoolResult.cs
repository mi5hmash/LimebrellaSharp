namespace LimebrellaSharpCore.Models;

public class BoolResult(bool result, string description = "")
{
    public bool Result { get; set; } = result;

    public string Description { get; set; } = description;
}