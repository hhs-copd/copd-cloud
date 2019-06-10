using System;
using Amazon.DynamoDBv2.Model;

namespace LambdaStoreFiles.CSV
{
    internal class HumidityItem : IItem
    {
        private const string StoredName = "Humidity";
        
        public string Name => StoredName;
        
        public DateTimeOffset DateTime { get; }

        public AttributeValue Value { get; }

        public HumidityItem(DateTimeOffset date, string value)
        {
            this.DateTime = date;
            this.Value = new AttributeValue
            {
                N = value
            };
        }
    }
}
