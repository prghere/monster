using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeSales.DataLayer
{
    public class SchoolDistrict
    {
        private string _schoolName;
        private int _schoolCode;

        public string SchoolName { get => _schoolName; set => _schoolName = value; }
        public int SchoolCode { get => _schoolCode; set => _schoolCode = value; }
    }

}