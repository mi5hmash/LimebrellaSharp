// v2024-08-03 21:16:48

namespace LimebrellaSharpCore.Models;

public class BoolResult(bool result, string description = "")
{
    public bool Result { get; set; } = result;

    public string Description { get; set; } = description;

    public void Set(bool result, string description)
    {
        Result = result;
        Description = description;
    }
}