using Amazon.DynamoDBv2.Model;
using System;

namespace LambdaStoreFiles.CSV
{
    internal class UVAItem : IItem
    {
        private const string StoredName = "UVA";

        public string Name => StoredName;

        public DateTimeOffset DateTime { get; }

        public AttributeValue Value { get; }

        public UVAItem(DateTimeOffset date, string value)
        {
            this.DateTime = date;
            this.Value = new AttributeValue
            {
                N = value
            };
        }
    }
}
