using DomainModel.Enum;

[AttributeUsage(AttributeTargets.Property)]
public class GlobalFormatAttribute : Attribute
{
    public FormatType Type { get; }

    public GlobalFormatAttribute(FormatType type)
    {
        Type = type;
    }
}
