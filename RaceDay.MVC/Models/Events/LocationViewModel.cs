
namespace RaceDay.MVC.Models.Events;

public class LocationViewModel
{
    public int LocationId { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Province { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}
