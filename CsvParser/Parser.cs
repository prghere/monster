using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HomeSales.DataLayer;

namespace HomeSales.CsvParser
{
    public class Parser
    {
        private readonly log4net.ILog logger 
            = log4net.LogManager.GetLogger(typeof(Parser));

        private readonly string fileName;
        private string[] fileContents;
        private int numOfLines;
        private int numOfThreads;
        private string delimitor;
        private Object yearmonthlock = new Object();
        private Object averageDaysLock = new Object();

        //TODO: need to put all these dictionaries in the distributed cache or static variables
        //TODO: data needs to be processed and initialized only once during setup or everytime file has changed
        //TODO: understand the issues with static variables in web farm
        public ConcurrentBag<HomeSale> listOfHomeSales = new ConcurrentBag<HomeSale>();
        public readonly ConcurrentDictionary<int, string> SchoolCodeNameMap 
            = new ConcurrentDictionary<int, string>();
        public readonly
            ConcurrentDictionary<int/*year*/,
                ConcurrentDictionary<int/*month*/,
                    ConcurrentDictionary<int/*schoolCode*/, decimal/*total property value*/>>> 
            YearMonthTotalValueSold = new ConcurrentDictionary<int, ConcurrentDictionary<
                int, ConcurrentDictionary<int, decimal>>>();

        public readonly ConcurrentDictionary<int/*year*/,
                ConcurrentDictionary<int/*month*/,
                    ConcurrentDictionary<int/*schoolCode*/, TotalDaysNumberOfSales/*calculate average no of days using it*/>>>
            AverageNumOfDays = new ConcurrentDictionary<int, ConcurrentDictionary<int, 
                ConcurrentDictionary<int, TotalDaysNumberOfSales>>>();

        // TODO:max lines and filename should be taken from web.config
        public Parser(string fileName)
        {
            // check whether file exists in the constructor so that all other places
            // we can assume it exists.
            if (!File.Exists(fileName))
            {
                throw new ArgumentException("file doesn't exist!");
            }
            this.fileName = fileName;
            // this assumes we know the number
            //numOfLines = NumberofLinesInFile();
            //Console.WriteLine("Number of lines in file: {0}", numOfLines);
            //fileContents = new String[numOfLines];
            Initialize();
        }

        private void ConstructListOfSales()
        {

        }

        private void Initialize()
        {
            //TODO: take these variables from the config
            // this needs to be benchmarked to find out what is the good default
            // its based on cores the computer has among other things
            numOfThreads = 1;
            delimitor = "\t";
        }

        // empty lines are not counted
        private int NumberofLinesInFile()
        {
            var lineCount = 0;
            using (var reader = File.OpenText(fileName))
            {
                string line = null;
                while ((line = reader.ReadLine() )!= null)
                {
                    var trimmedline = line.Trim();
                    if (trimmedline != string.Empty)
                    {
                        // only count non empty strings
                        lineCount++;
                    }
                }
            }

            return lineCount;
        }
        public void ReadFileIntoBuffer()
        {
            using (StreamReader file = new StreamReader(fileName))
            {
                string line = null;
                List<string> listOfLines = new List<string>();
                while ((line = file.ReadLine()) != null)
                {
                    var trimmedline = line.Trim();
                    if (trimmedline != string.Empty)
                    {
                        listOfLines.Add(trimmedline);
                    }
                }
                // array is efficient to do partition and as an input for parallelfor construct
                fileContents = listOfLines.ToArray();
                numOfLines = listOfLines.Count();
            }
        }

        public void Process()
        {
            ReadFileIntoBuffer();
            ProcessFileContents();
        }

        // this is a function for worker thread
        private void ProcessFileContents()
        {
            int linesPerThread = numOfLines / numOfThreads;
            if (linesPerThread <= 0)
            {
                // this can become zero of the number of lines is less than number of threads
                // set the number of threads to 1 because of very small input size
                numOfThreads = 1;
                linesPerThread = numOfLines;
            }
            Parallel.ForEach(Partitioner.Create(0, numOfLines, linesPerThread),
                (range) =>
                {

                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        // smaller, equally sized blocks of work
                        ProcessLineAndAddToConcurrentBag(fileContents[i]);
                    }
                });
        }

        private void ProcessLineAndAddToConcurrentBag(string line)
        {
            HomeSale sale = ProcessLine(line);
            if (sale != null)
            {
                listOfHomeSales.Add(sale);
                SchoolCodeNameMap.TryAdd(sale.SchoolCode, sale.SchoolDescription);
                UpdateYearMonthTotalSold(sale);
                UpdateAverageNumOfDays(sale);
            }
        }

        //we can make HomeSale as a struct for performance reasons
        // In that case, when bad input is in the line, throw exception.
        private HomeSale ProcessLine(string line)
        {
            //TODO: remove this line
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            // delimiter of this file is tab; so, thats what we will use;
            // We should take this delimitor from config as well


            string[] splitContent = line.Split(delimitor.ToCharArray());
            if (splitContent.Length != 6)
            {
                // log the line and return
                return null;
            }

            string propertyZip = splitContent[0];
            if (!int.TryParse(splitContent[1], out var schoolCode))
            {
                //log the issue and return; we will not consider this line
                return null;
            }

            string schoolName = splitContent[2];
            if (!DateTime.TryParse(splitContent[3], out var record))
            {
                //log the issue and return; we will not consider this line
                return null;
            }

            // we can add more date validations about the range of the value
            if (!DateTime.TryParse(splitContent[4], out var saleDt))
            {
                //log the issue and return; we will not consider this line
                return null;
            }

            // we can add more date validations about the range of the value
            if (!decimal.TryParse(splitContent[5], out var priceOfSale))
            {
                //log the issue and return; we will not consider this line
                return null;
            }

            HomeSale sale = new HomeSale(propertyZip, schoolCode, schoolName, record, saleDt, priceOfSale);
            return sale;
        }

        private void UpdateYearMonthTotalSold(HomeSale sale)
        {
            int year = sale.SaleDate.Year;
            int month = sale.SaleDate.Month;
            int schoolCode = sale.SchoolCode; // assumed as int

            // if year is not there, new dictionary is added. otherwise, we can assume value with the key is already there
            YearMonthTotalValueSold.TryAdd(
                year, new ConcurrentDictionary<int, ConcurrentDictionary<int, decimal>>());
            // if month is not there, new dictionary is added. otherwise, we can assume value with the key is already there
            YearMonthTotalValueSold[year]
                .TryAdd(month, new ConcurrentDictionary<int, decimal>());

            bool hasSchoolAdded = YearMonthTotalValueSold[year][month].TryAdd(schoolCode, sale.Price);
            if (!hasSchoolAdded)
            {
                // this school has already entry, update accumlated price 
                lock (yearmonthlock)
                {
                    YearMonthTotalValueSold[year][month][sale.SchoolCode] =
                        YearMonthTotalValueSold[year][month][sale.SchoolCode] + sale.Price;
                }
            }
        }

        private void UpdateAverageNumOfDays(HomeSale sale)
        {
            int year = sale.SaleDate.Year;
            int month = sale.SaleDate.Month;
            int schoolCode = sale.SchoolCode;
            int numberOfDays = (sale.RecordDate - sale.SaleDate).Days;

            AverageNumOfDays.TryAdd(year, new ConcurrentDictionary<int, ConcurrentDictionary<int, TotalDaysNumberOfSales>>());
            AverageNumOfDays[year].TryAdd(month, new ConcurrentDictionary<int, TotalDaysNumberOfSales>());
            //if this sale is the first one, we want to add 1, numberofdays
            bool hasAdded = AverageNumOfDays[year][month]
                .TryAdd(schoolCode, new TotalDaysNumberOfSales(1, numberOfDays));
            if (!hasAdded)
            {
                lock (averageDaysLock)
                {
                    TotalDaysNumberOfSales days = AverageNumOfDays[year][month][schoolCode];
                    TotalDaysNumberOfSales newValue = new TotalDaysNumberOfSales(days.NumberOfSales+1, days.TotalNumberOfDays+numberOfDays);
                    AverageNumOfDays[year][month][schoolCode] = newValue;
                }
            }
        }
    }
}
