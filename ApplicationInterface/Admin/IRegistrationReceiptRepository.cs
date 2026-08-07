using DomainModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IRegistrationReceiptRepository
    {
        Task<IEnumerable<RegistrationReceiptResponse>> GetRegistrationReceiptAsync(RegistrationReceiptRequest request);
    }
}
