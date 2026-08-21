using System.ComponentModel;

namespace DomainModel.Enum
{
    public enum FormatType
    {
        Currency,
        Number,
        Percentage,
        Date_ddMMyy,          // 21/08/26
        Date_ddMMyyyy_Dash,   // 21-08-2026
        Date_ddMMMMyyyy,      // 21 August 2026
        DateTime_Full          // 21/08/2026 14:30:45
    }
    public enum Gender
    {
        Male,
        Female,
        Other
    }
    public enum Initial
    {
        Mr,
        Mrs,
        Ms,
        Dr

    }

    public enum Qualification
    {
        [Description("10TH")]
        Tenth,
        [Description("12TH")]
        Twelth,
        [Description("GRADUATE")]
        GRADUATE,
        [Description("POST GRADUATE")]
        PG,
        [Description("Other")]
        Other
    }

    public enum PeriodType
    {
        Week,
        Month,
        Year,
        Days
    }
    public enum ComboType
    {
        Country,
        State,
        City,
        Gender,
        Category
    }
    public enum MessageType
    {
        SMS = 1,
        WhatsApp = 2,
        Email = 3
    }

}
