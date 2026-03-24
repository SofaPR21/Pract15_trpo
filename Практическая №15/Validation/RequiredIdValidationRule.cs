using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Практическая__15.Validation
{
    public class RequiredIdValidationRule : ValidationRule
    {
        public string FieldName { get; set; } = "Поле";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || (value is int intValue && intValue == 0))
            {
                return new ValidationResult(false, $"Необходимо выбрать {FieldName}");
            }
            return ValidationResult.ValidResult;
        }
    }
}
