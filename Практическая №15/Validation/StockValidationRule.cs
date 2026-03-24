using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Практическая__15.Validation
{
    public class StockValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                if (int.TryParse(value.ToString(), out int stock))
                {
                    if (stock < 0)
                        return new ValidationResult(false, "Количество не может быть отрицательным");

                    if (stock == 0)
                        return new ValidationResult(false, "Количество должно быть больше 0");

                    if (stock > 99999)
                        return new ValidationResult(false, "Количество слишком большое");
                }
                else
                {
                    return new ValidationResult(false, "Количество должно быть целым числом");
                }
            }
            else
            {
                return new ValidationResult(false, "Количество не может быть пустым");
            }
            return ValidationResult.ValidResult;
        }
    }
}
