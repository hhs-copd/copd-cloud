using System;
using System.Linq;
using Amazon.DynamoDBv2.Model;

namespace LambdaStoreFiles.CSV
{
    internal class ParticulateMatterItem : IItem
    {
        private const string StoredName = "ParticulateMatter";

        public string Name => StoredName;

        public DateTimeOffset DateTime { get; }

        public AttributeValue Value { get; }

        public ParticulateMatterItem(DateTimeOffset date, string value)
        {
            this.DateTime = date;
            this.Value = new AttributeValue
            {
                NS = value.Split(";").ToList()
            };
        }
    }
}