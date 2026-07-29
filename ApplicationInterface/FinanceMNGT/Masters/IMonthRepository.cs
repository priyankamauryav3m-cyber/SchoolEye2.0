using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IMonthRepository
    {
       public  Task<string> AddUpdateMonth(MonthModel month);
       public  Task<int> DeleteMonthData(int Sid);
        public Task<IEnumerable<MonthModel>> GetMonthData();
    }
}
