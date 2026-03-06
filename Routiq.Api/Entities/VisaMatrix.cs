namespace Routiq.Api.Entities;

public class VisaMatrix
{
    public Guid Id { get; set; }

    // Passport Origin
    public string PassportCountry { get; set; } = string.Empty;

    // Destination 
    public string DestinationCountry { get; set; } = string.Empty;

    // Visa Requirements: "VisaFree", "eVisa", "Required"
    public string VisaStatus { get; set; } = string.Empty;
}
