using HomeSales.DataLayer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Metadata.Providers;
using HomeSales.DataLayer;
using HomeSales.Models;

namespace HomeSales.Controllers
{

    public class SalesController : ApiController
    {

        private static int processOnce = 0;
        private static CacheData cacheHomeSales = null;
        private Object processOnceLock = new Object();

        private void ProcessSetup()
        {
            lock (processOnceLock)
            {
                Interlocked.Increment(ref processOnce);
                if(processOnce == 0)
                    return;

                //TODO:take this from appsettings in web.config
                string path = Path.Combine("App_Data",
                    @"home-sales.csv");
                //string webCurrentDirectory ;
                String fullpath = HttpContext.Current.Server.MapPath(@"~/App_Data/home-sales.csv");
                CsvParser.Parser parser = new CsvParser.Parser(fullpath);
                parser.Process();
                cacheHomeSales = new CacheData(
                    parser.listOfHomeSales, parser.SchoolCodeNameMap,
                    parser.YearMonthTotalValueSold, parser.AverageNumOfDays
                );
            }
        }

        private readonly log4net.ILog logger
            = log4net.LogManager.GetLogger(typeof(SalesController));

        /// <summary>
        /// Because it is a small file, we process it and keept the processed data in memory.
        /// Otherwise, we will have to process it while reading the file with out storing everything
        /// things to do:
        /// 1. unit testing.
        /// 2. inject the CacheData in the constructor and do unit testing.
        /// 3. handling exceptions. didn't have time to do that.
        /// *****This Code is not tested properly. didn't spent much time. just finished in the weekend.
        /// For a given a year and month, find top N distinct schools (display name and code) 
        /// with the largest dollar value of all property sold 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        [HttpGet]
        public List<SchoolDistrict> GetSchoolDistrictsWithTopSales(int year, int month, int n)
        {
            logger.Debug("GetSchoolDistrictsWithTopSales() entered");

            if (processOnce == 0)
            {
                ProcessSetup();
            }

            if (!cacheHomeSales.YearMonthTotalValueSold.ContainsKey(year))
            {
                return new List<SchoolDistrict>();
            }
            if (!cacheHomeSales.YearMonthTotalValueSold[year].ContainsKey(month))
            {
                return new List<SchoolDistrict>();
            }



            ConcurrentDictionary<int /*schoolCode*/, decimal /*total property value*/>
                schoolDictionary =
                    cacheHomeSales.YearMonthTotalValueSold[year][month];
            List<int> schoolCodeList =
                (from entry in schoolDictionary
                    orderby entry.Value descending
                    select entry.Key
                ).Take(n).ToList();

            return 
            (
                from sc in schoolCodeList
                from sd in cacheHomeSales.SchoolCodeNameMap
                where sc.Equals(sd.Key)
                select new SchoolDistrict {SchoolCode = sd.Key, SchoolName = sd.Value}
            ).ToList();
        }

        [HttpPost]
        public TotalDaysNumberOfSales AvgNumberOfDaysFromRecordToRegister(SchoolYearMonth sym)
        {
            if (processOnce == 0)
            {
                ProcessSetup();
            }

            if (!cacheHomeSales.AverageNumOfDays.ContainsKey(sym.Year))
            {
                return new TotalDaysNumberOfSales();
            }
            if (!cacheHomeSales.AverageNumOfDays[sym.Year].ContainsKey(sym.Month))
            {
                return new TotalDaysNumberOfSales();
            }
            if (!cacheHomeSales.AverageNumOfDays[sym.Year][sym.Month].ContainsKey(sym.SchoolCode))
            {
                return new TotalDaysNumberOfSales();
            }
            return cacheHomeSales.AverageNumOfDays[sym.Year][sym.Month][sym.SchoolCode];
        }
    }
}
