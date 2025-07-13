using CardGeneratorBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace CardGeneratorBackend.DTO.Validation
{
    public class RequiredWithCardVariant(CardVariant variant) : ValidationAttribute
    {
        private CardVariant Variant { get; init; } = variant;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var variantField = validationContext.ObjectType.GetProperty("Variant", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if(variantField is null || variantField.PropertyType != typeof(CardVariant))
            {
                return new ValidationResult("Object does not have a valid variant field");
            }

            var cardVariant = variantField.GetValue(validationContext.ObjectInstance);

            if(cardVariant is null)
            {
                return new ValidationResult("Object does not have a valid variant field");
            }

            if ((CardVariant)cardVariant != Variant)
            {
                return ValidationResult.Success;
            }

            return value is null ? new ValidationResult($"With card variant {Variant}, {validationContext.DisplayName} should not be null") : ValidationResult.Success;
        }
    }
}
