using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DomainModel.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ServerWebUI.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ServerWebUI.Components.CommonClass
{
    public class CommonMethod : ICommonMethod
    {
        private readonly IJSRuntime JS;
        private readonly IHttpService _http;


        private List<SessionModel> sessionlist = new();
        public string sessionvalue { get; set; }
        public CommonMethod(IJSRuntime jS, IHttpService http)
        {

            JS = jS;
            _http = http;
        }
        public string CalculateAge(DateTime dob)
        {
            if (dob > DateTime.Today)
            {
                return null;
            }
                var today = DateTime.Today;

            int years = today.Year - dob.Year;
            int months = today.Month - dob.Month;
            int days = today.Day - dob.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(today.Year, today.Month == 1 ? 12 : today.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            return $"{years} Year {months} Month {days} Day";
        }
        public async Task OpenDateFrom(ElementReference dobWrapper)    
        {

            await JS.InvokeVoidAsync("openDatePickerFromWrapper", dobWrapper);

        }
        public async Task<string> SetCurrentSession()
        {
            string ApiUri1 = "Session/GetSession";
            sessionlist = await _http.Get<List<SessionModel>>(ApiUri1) ?? new();
            var today = DateTime.Now;

            string currentSession;

            // Example: Session starts in July
            if (today.Month >= 4)
                currentSession = $"{today.Year}-{today.Year + 1}";
            else
                currentSession = $"{today.Year - 1}-{today.Year}";
            string normalize(string s) => s.Replace(" ", "").Replace("/", "-").Trim();

            var match = sessionlist.FirstOrDefault(x => normalize(x.SessionName) == normalize(currentSession));


            if (match != null)
            {
                sessionvalue = match.SessionName;
                return sessionvalue;
            }
            return "";
        }
        public async Task<long> SetCurrentSessionData()
        {
            string ApiUri1 = "Session/GetSession";
            sessionlist = await _http.Get<List<SessionModel>>(ApiUri1) ?? new();

            var today = DateTime.Now;
            string currentSession;

            // April se new session start
            if (today.Month >= 4)
                currentSession = $"{today.Year}-{today.Year + 1}";
            else
                currentSession = $"{today.Year - 1}-{today.Year}";

            string normalize(string s) => s.Replace(" ", "").Replace("/", "-").Trim();

            var match = sessionlist
                .FirstOrDefault(x => normalize(x.SessionName) == normalize(currentSession));

            if (match != null)
            {
                return match.SessionId;   // ✅ ID return karo
            }

            return 0; // default
        }
        public async Task<string> GetCurrentSession()
        {
            var today = DateTime.Today;
            int year = today.Year;

            // Academic session starts from April 1
            if (today.Month >= 4) // April or later
            {
                int startYear = year;
                int endYear = year + 1;
                return $"{startYear}-{endYear}";
            }
            else // January, February, March → still previous session
            {
                int startYear = year - 1;
                int endYear = year;
                return $"{startYear}-{endYear}";
            }
        }
        public bool IsEdited(object obj, string datamatch)
        {
            if (datamatch == "")
                return false;

            return CreateSnapshot(obj) != datamatch;
        }

        public string CreateSnapshot(object obj)
        {
            return System.Text.Json.JsonSerializer.Serialize(
                obj,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = false
                });
        }
    }
}
