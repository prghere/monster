using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeSales.DataLayer
{
    public class HomeSale
    {
        private string _propertyZipCodeCode;
        private int schoolCode;
        private string schoolDescription;
        private DateTime recordDate;
        private DateTime saleDate;
        private Decimal price;

        public HomeSale(
            string propertyZipCode,
            int schoolCode,
            string schoolDescription,
            DateTime recordDate,
            DateTime saleDate,
            decimal price
            ) 
        {
            PropertyZipCode = propertyZipCode;
            SchoolCode = schoolCode;
            SchoolDescription = schoolDescription;
            RecordDate = recordDate;
            SaleDate = saleDate;
            Price = price;
        }

        public string PropertyZipCode { get => _propertyZipCodeCode; set => _propertyZipCodeCode = value; }
        public int SchoolCode { get => schoolCode; set => schoolCode = value; }
        public string SchoolDescription { get => schoolDescription; set => schoolDescription = value; }
        public DateTime RecordDate { get => recordDate; set => recordDate = value; }
        public DateTime SaleDate { get => saleDate; set => saleDate = value; }
        public decimal Price { get => price; set => price = value; }
    }

}