using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerWebUI.Components.CommonClass
{
    public interface ICommonMethod
    {
        public string CalculateAge(DateTime dob);
        public  Task OpenDateFrom(ElementReference dobWrapper);
        public Task<string> SetCurrentSession();
        public Task<long> SetCurrentSessionData();
        public Task<string> GetCurrentSession();
        bool IsEdited(object obj, string datamatch);
        public string CreateSnapshot(object obj);

    }
}
