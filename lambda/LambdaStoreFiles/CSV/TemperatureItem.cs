using System;
using Amazon.DynamoDBv2.Model;

namespace LambdaStoreFiles.CSV
{
    internal class TemperatureItem : IItem
    {
        private const string StoredName = "Temperature";
        
        public string Name => StoredName;
        
        public DateTimeOffset DateTime { get; }

        public AttributeValue Value { get; }

        public TemperatureItem(DateTimeOffset date, string value)
        {
            this.DateTime = date;
            this.Value = new AttributeValue
            {
                N = value
            };
        }
    }
}
