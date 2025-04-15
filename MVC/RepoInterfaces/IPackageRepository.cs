using mvc.Models;

namespace mvc.RepoInterfaces
{
    public interface IPackageRepository: IGeniricRepository<int, Package>
    {
        bool IsExist(string name);
    }
}
