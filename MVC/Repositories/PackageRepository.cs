using mvc.Models;
using mvc.RepoInterfaces;
using mvc.Models;
using Polly;

namespace mvc.Repositories
{
    public class PackageRepository : GeniricRepository<int, Package>, IPackageRepository, IGeniricRepository<int, Package>
    {
        public PackageRepository(ProjectContext context) : base(context)
        {
        }
        public bool IsExist(string name)
        {
            return dbSet.Any(p=>p.Name==name);
        }

    }
    
    
}
