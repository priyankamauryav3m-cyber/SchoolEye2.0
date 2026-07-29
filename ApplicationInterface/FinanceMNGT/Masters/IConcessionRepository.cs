using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IConcessionRepository
    {
        public Task<string> AddUpdateConcession(ConcessionModel concession);
        public Task<int> DeleteConcessionData(int cid);
        public Task<IEnumerable<ConcessionModel>> GetConcessionData();
        public Task<IEnumerable<StudentWithConcessionDto>> GetStudentWithConcessionAsync(StudentConcessionFilterDto filter);
       public Task<IEnumerable<StudentWithConcessionDto>> GetSearchStudent(SearchAnyRequestModel searchAny);
        public Task<string> AddOrUpdateFeeheadConcessionData(List<ConcessionFeehead> model);
        public Task<string>  SaveStudentConcession(StudentConcessionDto model);
        public Task<int> UpdateStudentConcessionRemarksData(StudentConcessionRemarks concession);
       public Task<int> ManageConcessionAsync(ConcessionManageRequest request);
      public  Task<List<StudentMappedConcessionDto>> GetStudentMappedConcession(SearchAnyRequestModel searchAnyRequest);
        public Task<int> UnMapConcessionWithStudentAsync(UnMapConcessionRequest request);





    }
}
