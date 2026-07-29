using DomainModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SuperAdmin
{
    public interface ISendSMS
    {
        public  Task<string> SaveSMSSentDetails(SMSSentModel model);
        Task<bool> SendSmsAsync(string mobileNo, string message);
    }
}
