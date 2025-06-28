using System.ComponentModel.DataAnnotations;

namespace AppHospedagemAPI.ValidationAttributes;

public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _propertyName;
    private readonly object _desiredValue;

    public RequiredIfAttribute(string propertyName, object desiredValue)
    {
        _propertyName = propertyName;
        _desiredValue = desiredValue;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(_propertyName);
        if (property == null)
        {
            throw new ArgumentException("Propriedade dependente não encontrada.");
        }

        var propertyValue = property.GetValue(validationContext.ObjectInstance);

        if (propertyValue != null && propertyValue.Equals(_desiredValue))
        {
            if (value == null || (value is string strValue && string.IsNullOrWhiteSpace(strValue)))
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} é obrigatório.");
            }
        }
        return ValidationResult.Success;
    }
}