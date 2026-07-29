using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface ViewStudentFeeHeadRepository
    {
        public Task<string> StudentCopyHeadData(StudenmapheadModal stu);
        public Task<List<MapwithFeehead>> GetSearchedStudentData(MapwithFeehead model);
        public Task<List<MapwithFeehead>> GetStudentMappedWithFeeHead(MapwithFeehead model);
        public Task<UnMapFeeHead> UnMapFeeHeadWithStudent(UnMapFeeHead model);
    }
}

