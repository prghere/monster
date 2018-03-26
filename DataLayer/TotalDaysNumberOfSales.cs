using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSales.DataLayer
{
    public struct TotalDaysNumberOfSales
    {
        private int numberOfSales;
        private int totalNumberOfDays;

        public TotalDaysNumberOfSales(int noOfSales, int totaldays)
        {
            numberOfSales = noOfSales;
            totalNumberOfDays = totaldays;
        }

        public int NumberOfSales
        {
            get { return numberOfSales; }
        }

        public int TotalNumberOfDays
        {
            get { return totalNumberOfDays; }
        }
    }
}
