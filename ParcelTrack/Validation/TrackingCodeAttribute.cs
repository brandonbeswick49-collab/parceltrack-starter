using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ParcelTrack.Validation
{
    public class TrackingCodeAttribute : ValidationAttribute
    {
        public TrackingCodeAttribute()
        {
            ErrorMessage = "Tracking code must look like PT-123456";
        }

        public override bool IsValid(object? value)
        {
            string? text = value as string;
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            return Regex.IsMatch(text, @"^PT-\d{6}$");
        }
    }
}