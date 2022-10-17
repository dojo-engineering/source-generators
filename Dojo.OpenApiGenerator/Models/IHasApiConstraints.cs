namespace Dojo.OpenApiGenerator.Models
{
    internal interface IHasApiConstraints
    {
        bool IsRequired { get; }
        int? MaxLength { get; }
        int? MinLength { get; }
    }
}
