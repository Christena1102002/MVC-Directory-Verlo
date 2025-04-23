using mvc.Models;
using mvc.RepoInterfaces;
using mvc.Models;
using System.Linq.Expressions;

namespace mvc.Repositories
{
    public class BusinessFeaturesRepoisitory : GeniricRepository<int, BusinessFeatures>,IBusinessFeaturesRepoisitory,IGeniricRepository<int, BusinessFeatures>
    {
        public BusinessFeaturesRepoisitory(ProjectContext context) : base(context)
        {

        }
    }
}
