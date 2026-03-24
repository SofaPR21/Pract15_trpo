using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace Практическая__15.Validation
{
    public class PriceRangeValidationRule : ValidationRule
    {
        public string FromPrice { get; set; }
        public string ToPrice { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.ValidResult;
            }

            string input = value.ToString().Trim();
            input = input.Replace(',', '.');

            if (!decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal currentPrice))
            {
                return new ValidationResult(false, "Введите корректное число");
            }

            if (currentPrice < 0)
            {
                return new ValidationResult(false, "Цена не может быть отрицательной");
            }
            if (currentPrice > 9999999.99m)
            {
                return new ValidationResult(false, "Цена не может превышать 9,999,999.99");
            }

            return ValidationResult.ValidResult;
        }
    }

    public class PriceCompareValidationRule : ValidationRule
    {
        public string ComparisonType { get; set; } 

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return ValidationResult.ValidResult;
        }
    }
}
