namespace CloudComBook.Shared.Filters;

public class AnymalFilter
{
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Village { get; set; }

    public bool HasCovs { get; set; }
    public bool HasHorses { get; set; }
    public bool HasPigs { get; set; }
    public bool HasSheeps { get; set; }
    public bool HasGoats { get; set; }
    public bool HasBirds { get; set; }
    public bool HasRabbits { get; set; }
    public bool HasBeeses { get; set; }
}
