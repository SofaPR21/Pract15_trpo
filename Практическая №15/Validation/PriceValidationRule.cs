using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Практическая__15.Validation
{
    public class PriceValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                string input = value.ToString();
                input = input.Replace(',', '.');

                if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                {
                    if (price <= 0)
                        return new ValidationResult(false, "Цена должна быть больше 0");

                    if (price > 9999999.99m)
                        return new ValidationResult(false, "Цена слишком высокая");
                }
                else
                {
                    return new ValidationResult(false, "Введите число (например: 1000.50)");
                }
            }
            else
            {
                return new ValidationResult(false, "Цена не может быть пустой");
            }

            return ValidationResult.ValidResult;
        }
    }
}
