using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface ITransportFeeConfigRepository
    {
        public Task<string> AddOrUpdateTransportData(TransportFeeConfig transport);
        public Task<int> DeleteTransportData(int tid);
        public Task<IEnumerable<TransportFeeConfig>> GetTransporData();
        public Task<IEnumerable<TransportFeeConfig>> GetDistanceMapAmount(long SessionId);
    }
}
