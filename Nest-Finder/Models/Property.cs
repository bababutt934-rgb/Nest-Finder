namespace NestFinder.Models;

public class Property
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Type { get; set; } = "Residential";
    public decimal Price { get; set; }
    public string Status { get; set; } = "Available";
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int AreaSqFt { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual string PropertyType => Type;
}

public class ResidentialProperty : Property
{
    public ResidentialProperty() { Type = "Residential"; }
}

public class CommercialProperty : Property
{
    public string BusinessType { get; set; } = string.Empty;
    public CommercialProperty() { Type = "Commercial"; }
}
