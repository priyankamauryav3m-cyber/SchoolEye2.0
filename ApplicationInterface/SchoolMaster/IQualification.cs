using DomainModel.SchoolMaster;


namespace ApplicationInterface.SchoolMaster
{
    public interface IQualification
    {
      
        public Task<IEnumerable<Qualification>> GetAllQualification();
        public Task<string> AddUpdateQualification(Qualification objqualification);
        public Task<int> DeleteQualification(int Id);
    }
}
