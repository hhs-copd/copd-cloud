using System;

namespace LambdaStoreFiles.CSV
{
    internal class UVCSVItem : ICSVItem
    {
        public DateTimeOffset DateTime { get; }

        public float Value { get; }

        public UVCSVItem(DateTimeOffset date, string sensor, string value)
        {
            this.DateTime = date;
            this.Value = float.Parse(value);
        }
    }
}
