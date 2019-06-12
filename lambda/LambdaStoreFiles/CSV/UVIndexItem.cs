using Amazon.DynamoDBv2.Model;
using System;
using System.Linq;

namespace LambdaStoreFiles.CSV
{
    internal class UVIndexItem : IItem
    {
        private const string StoredName = "UVIndex";

        public string Name => StoredName;

        public DateTimeOffset DateTime { get; }

        public AttributeValue Value { get; }

        public UVIndexItem(DateTimeOffset date, string value)
        {
            this.DateTime = date;
            this.Value = new AttributeValue
            {
                NS = value.Split(';').ToList()
            };
        }
    }
}
