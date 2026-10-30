namespace CloudComBook.Shared.Models;

public class Community
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;

    // заповнюються лише у списку громад
    public int UsersCount { get; set; }
    public int VillagesCount { get; set; }
}