using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HomeSales.CsvParser;

namespace ConsoleTest
{
    public class TestFunctions
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello");

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"home-sales.csv");
                Console.WriteLine(path);
            Console.ReadKey();

            Parser parser = new Parser(path);
            parser.ReadFileIntoBuffer();
            parser.Process();
        }
    }
}
