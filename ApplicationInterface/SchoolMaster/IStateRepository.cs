using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IStateRepository
    {
       

        public Task<IEnumerable<StateModel>> GetAllAsync();

        

        public Task<int> DeleteStateData(int stateId);
        public Task<string> AddUpdateState(StateModel objState);
    }

}
