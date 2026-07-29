using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public  interface IClassSectionRepository
    {
        public Task<IEnumerable<ClassSectionModal>> GetClassSectionData();
        public Task<IEnumerable<CategoryModal>> GetCategoryData();
        public Task<IEnumerable<MstDistance>> GetDistanceData();
        public Task<IEnumerable<MotherTongue>> GetMotherTongueData();
        public Task<IEnumerable<VisaType>> GetVisaTypeData();
        public Task<IEnumerable<PassportName>> GetPassportTypeNameData();
        public Task<IEnumerable<BranchNameMst>> GetBranchNameData();
        public Task<List<SectionModel>> GetClassSection(SearchAnyRequestModel model);
    }
}
