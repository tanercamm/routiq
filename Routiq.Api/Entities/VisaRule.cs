using System.ComponentModel.DataAnnotations;

namespace Routiq.Api.Entities;

public enum VisaType
{
    VisaFree,
    Evisa,
    OnArrival,
    Required
}

public class VisaRule
{
    public int Id { get; set; }

    [Required]
    public string PassportCountry { get; set; } = string.Empty;

    [Required]
    public string DestinationCountry { get; set; } = string.Empty;

    public VisaType VisaType { get; set; }

    public int MaxStayDays { get; set; }
}
