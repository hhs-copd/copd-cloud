using Amazon.DynamoDBv2.Model;
using System;

namespace LambdaStoreFiles.CSV
{
    internal class PM10p0Item : IItem
    {
        private const string StoredName = "PM10p0";

        public string Name => StoredName;

        public DateTimeOffset DateTime { get; }

        public AttributeValue Value { get; }

        public PM10p0Item(DateTimeOffset date, string value)
        {
            this.DateTime = date;
            this.Value = new AttributeValue
            {
                N = value
            };
        }
    }
}
