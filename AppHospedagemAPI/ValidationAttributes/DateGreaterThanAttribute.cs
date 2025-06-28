using System.ComponentModel.DataAnnotations;

namespace AppHospedagemAPI.ValidationAttributes;

public class DateGreaterThanAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateGreaterThanAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var currentValue = value as DateTime?;

        var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
        if (property == null)
        {
            throw new ArgumentException("Propriedade de comparação não encontrada.");
        }

        var comparisonValue = property.GetValue(validationContext.ObjectInstance) as DateTime?;

        if (currentValue.HasValue && comparisonValue.HasValue && currentValue.Value <= comparisonValue.Value)
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} deve ser posterior a {property.Name}.");
        }

        return ValidationResult.Success;
    }
}