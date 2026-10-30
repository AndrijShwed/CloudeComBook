using CloudComBook.Shared.Models;

namespace CloudComBook.API.Repositories.Interfaces;

public class CommunityUsage
{
    public int Users { get; set; }
    public int Villages { get; set; }
    public int Streets { get; set; }
    public int Plots { get; set; }
    public int Anymals { get; set; }
    public int Total => Users + Villages + Streets + Plots + Anymals;
}

public interface ICommunityRepository
{
    Task<IEnumerable<Community>> GetAllAsync();
    Task<Community?> GetByIdAsync(int id);
    Task<int> CreateAsync(Community community);
    Task<bool> UpdateAsync(Community community);
    Task<bool> DeleteAsync(int id);
    Task<CommunityUsage> GetUsageAsync(int id);
}
