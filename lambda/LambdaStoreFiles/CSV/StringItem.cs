using Amazon.DynamoDBv2.Model;
using System;

namespace LambdaStoreFiles.CSV
{
    internal class StringItem : IItem
    {
        public string Name { get; }

        public DateTimeOffset DateTime { get; }

        public AttributeValue Value { get; }

        public StringItem(DateTimeOffset date, string sensor, string value)
        {
            this.Name = sensor;
            this.DateTime = date;
            this.Value = new AttributeValue
            {
                N = value
            };
        }
    }
}
