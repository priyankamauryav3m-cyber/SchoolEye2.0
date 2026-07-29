using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Common
{
    public class ComboCommonDomain
    {

        public class TwoFieldDomain
        {
            public string? ValueField { get; set; } 
            public string? TextField { get; set; }
        }

        public class ThreeFieldDomain
        {
            public string? ValueField { get; set; }
            public string? IsNormalValueField { get; set; }
            public string? TextField { get; set; }
        }
        public class CompleteAddress
        {
            public string? ClusterId { get; set; }
            public string? ClusterName { get; set; }
            public string? PostalCode { get; set; }
            public string? CityId { get; set; }
            public string? CityName { get; set; }
            public string? DistrictId { get; set; }
            public string? DistrictName { get; set; }
            public string? CountryId { get; set; }
            public string? CountryName { get; set; }
        }
    }
}
