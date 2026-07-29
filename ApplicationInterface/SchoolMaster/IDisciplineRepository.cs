using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface IDisciplineRepository
    {

        public Task<IEnumerable<DisciplineModel>> GetAllAsync();

        // public Task<int> AddUpdateDiscipline(DisciplineModel objDiscipline);
        public Task<string> AddUpdateDiscipline(DisciplineModel objDiscipline);

        public Task<int> DeleteDisciplineData(int disciplineId);
    }
}
