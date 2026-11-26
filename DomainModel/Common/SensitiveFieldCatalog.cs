using System.Globalization;
using System.Text.RegularExpressions;

namespace DomainModel.Common
{
    // DPDP: kind of a sensitive value -> how it is validated and masked
    public enum SensitiveFieldType
    {
        Mobile,       // 10 digit mobile
        Phone,        // office / landline number
        Email,
        Aadhaar,
        Date,         // date of birth (yyyy-MM-dd)
        Text,         // health / disability notes
        Address,
        PinCode,
        BankAccount,
        Ifsc,
        Amount,       // annual income
        BloodGroup
    }

    public sealed record SensitiveFieldInfo(
        string Key,               // same key in the API, the page and USP_DPDP_UpdateStudentSensitiveDetails
        string Label,
        string Group,             // section on the Student Sensitive Details page
        SensitiveFieldType Type,
        int MaxLength,
        string Table);

    // Every student / parent field treated as sensitive personal data (DPDP).
    // These fields are masked + read-only on every screen and changed only on the
    // Student Sensitive Details page (/StudentSensitiveDetails).
    public static class SensitiveFieldCatalog
    {
        public const string PageUrl = "/StudentSensitiveDetails";

        public const string GroupStudent = "Student";
        public const string GroupHealth = "Health";
        public const string GroupFather = "Father";
        public const string GroupMother = "Mother";
        public const string GroupGuardian = "Guardian & Emergency";
        public const string GroupCurrentAddress = "Current Address";
        public const string GroupPermanentAddress = "Permanent Address";
        public const string GroupBank = "Bank";

        public static readonly string[] BloodGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

        private const string SM = "StudentMaster", SP = "StudentParent", SA = "StudentAddress", AI = "StudentAdditionalInformation";

        public static readonly IReadOnlyList<SensitiveFieldInfo> Fields = new List<SensitiveFieldInfo>
        {
            new("DateOfBirth",        "Date of Birth",        GroupStudent, SensitiveFieldType.Date,    10,  SM),
            new("AadhaarNo",          "Aadhaar No.",          GroupStudent, SensitiveFieldType.Aadhaar, 12,  SM),
            new("MobileNo",           "Mobile No.",           GroupStudent, SensitiveFieldType.Mobile,  10,  SM),
            new("EmailId",            "Email",                GroupStudent, SensitiveFieldType.Email,   100, SM),

            new("BloodGroup",         "Blood Group",          GroupHealth, SensitiveFieldType.BloodGroup, 5,   SM),
            new("MedicalInformation", "Medical Information",  GroupHealth, SensitiveFieldType.Text,       150, SM),
            new("NatureOfDisability", "Nature of Disability", GroupHealth, SensitiveFieldType.Text,       200, SM),

            new("FatherContactNo",       "Mobile No.",          GroupFather, SensitiveFieldType.Mobile,  10,  SP),
            new("FatherEMail",           "Email",               GroupFather, SensitiveFieldType.Email,   100, SP),
            new("FatherAadhaarNo",       "Aadhaar No.",         GroupFather, SensitiveFieldType.Aadhaar, 12,  SP),
            new("FatherDOB",             "Date of Birth",       GroupFather, SensitiveFieldType.Date,    10,  SP),
            new("FatherAnnualIncome",    "Annual Income (₹)",   GroupFather, SensitiveFieldType.Amount,  12,  SP),
            new("FatherOfficeContactNo", "Office Contact No.",  GroupFather, SensitiveFieldType.Phone,   15,  SP),
            new("FatherOfficeAddress",   "Office Address",      GroupFather, SensitiveFieldType.Address, 200, SP),

            new("MotherContactNo",       "Mobile No.",          GroupMother, SensitiveFieldType.Mobile,  10,  SP),
            new("MotherEMail",           "Email",               GroupMother, SensitiveFieldType.Email,   100, SP),
            new("MotherAadhaarNo",       "Aadhaar No.",         GroupMother, SensitiveFieldType.Aadhaar, 12,  SP),
            new("MotherDOB",             "Date of Birth",       GroupMother, SensitiveFieldType.Date,    10,  SP),
            new("MotherAnnualIncome",    "Annual Income (₹)",   GroupMother, SensitiveFieldType.Amount,  12,  SP),
            new("MotherOfficeContactNo", "Office Contact No.",  GroupMother, SensitiveFieldType.Phone,   15,  SP),
            new("MotherOfficeAddress",   "Office Address",      GroupMother, SensitiveFieldType.Address, 200, SP),

            new("SMSMobileNo",              "SMS Mobile No.",              GroupGuardian, SensitiveFieldType.Mobile,  10,  SP),
            new("GuardianContactNo",        "Guardian Mobile No.",         GroupGuardian, SensitiveFieldType.Mobile,  10,  SP),
            new("GuardianEmail",            "Guardian Email",              GroupGuardian, SensitiveFieldType.Email,   50,  SP),
            new("GuardianAddress",          "Guardian Address",            GroupGuardian, SensitiveFieldType.Address, 150, SP),
            new("EmergencyPersonContactNo", "Emergency Contact No.",       GroupGuardian, SensitiveFieldType.Mobile,  10,  SP),
            new("EmergencyPersonAddress",   "Emergency Contact Address",   GroupGuardian, SensitiveFieldType.Address, 100, SP),

            new("CurrentLine1",     "Address Line 1", GroupCurrentAddress, SensitiveFieldType.Address, 150, SA),
            new("CurrentLine2",     "Address Line 2", GroupCurrentAddress, SensitiveFieldType.Address, 150, SA),
            new("CurrentPinCode",   "Pin Code",       GroupCurrentAddress, SensitiveFieldType.PinCode, 6,   SA),
            new("CurrentContactNo", "Contact No.",    GroupCurrentAddress, SensitiveFieldType.Mobile,  10,  SA),

            new("PermanentLine1",     "Address Line 1", GroupPermanentAddress, SensitiveFieldType.Address, 150, SA),
            new("PermanentLine2",     "Address Line 2", GroupPermanentAddress, SensitiveFieldType.Address, 150, SA),
            new("PermanentPinCode",   "Pin Code",       GroupPermanentAddress, SensitiveFieldType.PinCode, 6,   SA),
            new("PermanentContactNo", "Contact No.",    GroupPermanentAddress, SensitiveFieldType.Mobile,  10,  SA),

            new("StudentBankAccount", "Bank Account No.", GroupBank, SensitiveFieldType.BankAccount, 18, AI),
            new("IFSC",               "IFSC Code",        GroupBank, SensitiveFieldType.Ifsc,        11, AI),
        };

        private static readonly Dictionary<string, SensitiveFieldInfo> ByKey =
            Fields.ToDictionary(f => f.Key, StringComparer.Ordinal);

        public static SensitiveFieldInfo? Find(string? key) =>
            key != null && ByKey.TryGetValue(key, out var f) ? f : null;

        public static string Mask(string key, string? value)
        {
            var f = Find(key);
            return f == null ? PiiMask.Hidden(value) : PiiMask.Mask(value, f.Type);
        }

        // trims and normalises a new value before validation / save (IFSC upper case, no spaces in numbers)
        public static string? Normalize(SensitiveFieldInfo field, string? value)
        {
            var v = value?.Trim();
            if (string.IsNullOrEmpty(v)) return null;
            return field.Type switch
            {
                SensitiveFieldType.Mobile or SensitiveFieldType.Aadhaar or SensitiveFieldType.PinCode
                    or SensitiveFieldType.BankAccount or SensitiveFieldType.Amount => Regex.Replace(v, @"\s+", ""),
                SensitiveFieldType.Ifsc or SensitiveFieldType.BloodGroup => v.ToUpperInvariant(),
                SensitiveFieldType.Email => v.Replace(" ", ""),
                _ => v
            };
        }

        // null = valid. An empty value is allowed (clears the field).
        public static string? Validate(SensitiveFieldInfo field, string? normalizedValue)
        {
            var v = normalizedValue;
            if (string.IsNullOrEmpty(v)) return null;
            if (v.Length > field.MaxLength) return $"{field.Label} can have at most {field.MaxLength} characters.";
            if (v.Any(char.IsControl)) return $"{field.Label} has invalid characters.";

            switch (field.Type)
            {
                case SensitiveFieldType.Mobile:
                    return Regex.IsMatch(v, ValidMobileNoAttribute.Pattern) ? null : "Enter valid 10 digit mobile number.";
                case SensitiveFieldType.Phone:
                    return Regex.IsMatch(v, @"^[0-9+\-\s]{6,15}$") ? null : "Enter valid contact number.";
                case SensitiveFieldType.Email:
                    return Regex.IsMatch(v, ValidEmailAttribute.Pattern) ? null : "Enter valid email.";
                case SensitiveFieldType.Aadhaar:
                    return Regex.IsMatch(v, ValidAadhaarNoAttribute.Pattern) ? null : "Enter valid 12 digit Aadhaar number.";
                case SensitiveFieldType.PinCode:
                    return Regex.IsMatch(v, @"^[1-9][0-9]{5}$") ? null : "Enter valid 6 digit pin code.";
                case SensitiveFieldType.Ifsc:
                    return Regex.IsMatch(v, @"^[A-Z]{4}0[A-Z0-9]{6}$") ? null : "Enter valid IFSC code (e.g. SBIN0001234).";
                case SensitiveFieldType.BankAccount:
                    return Regex.IsMatch(v, @"^[0-9]{9,18}$") ? null : "Enter valid bank account number (9 to 18 digits).";
                case SensitiveFieldType.Amount:
                    return long.TryParse(v, NumberStyles.None, CultureInfo.InvariantCulture, out var amt) && amt >= 0
                        ? null : "Enter a valid amount (whole rupees).";
                case SensitiveFieldType.BloodGroup:
                    return BloodGroups.Contains(v) ? null : "Select a valid blood group.";
                case SensitiveFieldType.Date:
                    if (!DateTime.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                        return "Enter a valid date.";
                    if (d.Date > DateTime.Today) return $"{field.Label} cannot be a future date.";
                    if (d.Year < 1900) return "Enter a valid date.";
                    return null;
                default:
                    return null;
            }
        }
    }
}
