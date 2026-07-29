using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface IMapFeeTemplateWithCourseRepository
    {
        
     public  Task<IEnumerable<ClassWiseFeeTemplateModel>> GetClassWiseFeeTemplate(SearchAnyRequestModel request);
     public Task<string> SaveOrUpdateClasswiseFeeTemplateData(ClassWiseFeeTemplateModel request);
    }
}
