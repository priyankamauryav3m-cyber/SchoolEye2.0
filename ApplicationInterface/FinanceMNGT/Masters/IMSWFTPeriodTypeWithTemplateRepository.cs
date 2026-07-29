using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface IMSWFTPeriodTypeWithTemplateRepository
    {
    

        public Task<List<IMSWFTPeriodType>> GetIMSWFTPeriodTypeData(IMSWFTPeriodType model);
   
       public  Task<bool> MapFeePeriodWithStudent(MapFeePeriodWithStudentModel model);
        public Task<List<SectionModel>> GetClassSection(SearchAnyRequestModel model);
        Task<bool> MapFeeTemplateWithStudent(MapFeePeriodWithStudentModel model);
    }
}
