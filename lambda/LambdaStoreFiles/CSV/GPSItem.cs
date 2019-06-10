using System;
using System.Linq;
using Amazon.DynamoDBv2.Model;

namespace LambdaStoreFiles.CSV
{
    internal class GPSItem : IItem
    {
        private const string StoredName = "GPS";
        
        public string Name => StoredName;
        
        public DateTimeOffset DateTime { get; }

        public AttributeValue Value { get; }

        public GPSItem(DateTimeOffset date, string value)
        {
            this.DateTime = date;
            this.Value = new AttributeValue
            {
                NS = value.Split(";").ToList()
            };
        }
    }
}