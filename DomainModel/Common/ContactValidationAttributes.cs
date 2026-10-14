using System.ComponentModel.DataAnnotations;

namespace DomainModel.Common
{
    // One rule for every email / mobile / Aadhaar field of the project.
    // Used by the Blazor forms (DataAnnotationsValidator: red message when the field is left)
    // and by the API ([ApiController] answers 400 for an invalid model), so both sides check the same.
    // An empty value is allowed — add [Required] where the field is mandatory.

    // name@domain.tld — any domain with any letter tld (.com, .in, .co.in, .org, .yahoo ...)
    public class ValidEmailAttribute : RegularExpressionAttribute
    {
        public const string Pattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}$";

        public ValidEmailAttribute() : base(Pattern)
        {
            ErrorMessage = "Enter valid email";
        }
    }

    // mobile / phone / alternate number: exactly 10 digits, starting with 6, 7, 8 or 9
    public class ValidMobileNoAttribute : RegularExpressionAttribute
    {
        public const string Pattern = @"^[6-9][0-9]{9}$";

        public ValidMobileNoAttribute() : base(Pattern)
        {
            ErrorMessage = "Enter valid 10 digit mobile number";
        }
    }

    // Aadhaar number: exactly 12 digits
    public class ValidAadhaarNoAttribute : RegularExpressionAttribute
    {
        public const string Pattern = @"^[0-9]{12}$";

        public ValidAadhaarNoAttribute() : base(Pattern)
        {
            ErrorMessage = "Enter valid 12 digit Aadhaar number";
        }
    }
}
