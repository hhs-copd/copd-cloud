using Amazon.DynamoDBv2.Model;
using System;

namespace LambdaStoreFiles.CSV
{
    internal class PM2p5Item : IItem
    {
        private const string StoredName = "PM2p5";

        public string Name => StoredName;

        public DateTimeOffset DateTime { get; }

        public AttributeValue Value { get; }

        public PM2p5Item(DateTimeOffset date, string value)
        {
            this.DateTime = date;
            this.Value = new AttributeValue
            {
                N = value
            };
        }
    }
}
