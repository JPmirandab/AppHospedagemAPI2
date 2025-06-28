using System.ComponentModel.DataAnnotations;

namespace AppHospedagemAPI.ValidationAttributes;

public class FutureOrTodayAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date.Date < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "A data não pode ser anterior à data atual.");
            }
        }
        return ValidationResult.Success;
    }
}