using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IMessageTypeRepository
    {
        public Task<IEnumerable<MessageTypeModal>> GetMessageType();
        public Task<IEnumerable<SmsEmailTextModel>> GetMessageType(int messageTypeId);
    }
}
