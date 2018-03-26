using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeSales.DataLayer
{
    public class CacheData
    {
        public CacheData(
            ConcurrentBag<HomeSale> listOfHomeSales,
            ConcurrentDictionary<int, string> schoolCodeNameMap,
            ConcurrentDictionary<int/*year*/,
                ConcurrentDictionary<int/*month*/,
                    ConcurrentDictionary<int/*schoolCode*/, decimal/*total property value*/>>>
                yearMonthTotalValueSold,
            ConcurrentDictionary<int/*year*/,
                ConcurrentDictionary<int/*month*/,
                    ConcurrentDictionary<int/*schoolCode*/, TotalDaysNumberOfSales/*calculate average no of days using it*/>>>
            averageNumOfDays 

        )
        {
            this.ListOfHomeSales = listOfHomeSales;
            this.SchoolCodeNameMap = schoolCodeNameMap;
            this.YearMonthTotalValueSold = yearMonthTotalValueSold;
            this.AverageNumOfDays = averageNumOfDays;
        }
        public readonly ConcurrentBag<HomeSale> ListOfHomeSales;
        public readonly ConcurrentDictionary<int, string> SchoolCodeNameMap;
        public readonly
            ConcurrentDictionary<int/*year*/,
                ConcurrentDictionary<int/*month*/,
             /*sorted list is better, but no cocurrent one available*/
                    ConcurrentDictionary<int/*schoolCode*/, decimal/*total property value*/>>>
            YearMonthTotalValueSold;

        public readonly ConcurrentDictionary<int/*year*/,
                ConcurrentDictionary<int/*month*/,
                    ConcurrentDictionary<int/*schoolCode*/, TotalDaysNumberOfSales/*calculate average no of days using it*/>>>
            AverageNumOfDays;
    }
}
