// v2024-10-05 21:16:48

namespace LimebrellaSharpCore.Models;

/// <summary>
/// Represents a result with a boolean value and a description.
/// </summary>
/// <param name="result"></param>
/// <param name="description"></param>
public class BoolResult(bool result, string description = "")
{
    /// <summary>
    /// Gets or sets the result of the operation.
    /// </summary>
    public bool Result { get; set; } = result;

    /// <summary>
    /// Gets or sets the description of the result.
    /// </summary>
    public string Description { get; set; } = description;

    /// <summary>
    /// Sets the result and description of the BoolResult.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    /// <param name="description">The description of the result.</param>
    public void Set(bool result, string description)
    {
        Result = result;
        Description = description;
    }
}