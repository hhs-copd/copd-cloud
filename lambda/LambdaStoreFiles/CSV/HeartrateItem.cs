using System;
using Amazon.DynamoDBv2.Model;

namespace LambdaStoreFiles.CSV
{
    internal class HeartrateItem : IItem
    {
        private const string StoredName = "Heartrate";
        
        public string Name => StoredName;
        
        public DateTimeOffset DateTime { get; }

        public AttributeValue Value { get; }

        public HeartrateItem(DateTimeOffset date, string value)
        {
            this.DateTime = date;
            this.Value = new AttributeValue
            {
                N = value
            };
        }
    }
}
