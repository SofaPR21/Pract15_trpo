using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Практическая__15.Validation
{
    public class ProductDescriptionValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Описание товара не может быть пустым");
            }

            string name = value.ToString();

            if (name.Length > 200)
                return new ValidationResult(false, "Описание товара не может превышать 200 символов");

            return ValidationResult.ValidResult;
        }
    }
}
