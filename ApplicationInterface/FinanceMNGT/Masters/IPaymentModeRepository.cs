using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IPaymentModeRepository
    {
        public Task<string> AddUpdatePaymentMode(PaymentModel paymentMode);
        public Task<int> DeletePaymentModeData(int pid);
        public Task<IEnumerable<PaymentModel>> GetPaymentModeData();
    }
}
