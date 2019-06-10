using System;

namespace LambdaStoreFiles.CSV
{
    internal class HeartrateCSVItem : ICSVItem
    {
        public DateTimeOffset DateTime { get; }

        public float Value { get; }

        public HeartrateCSVItem(DateTimeOffset date, string sensor, string value)
        {
            this.DateTime = date;
            this.Value = float.Parse(value);
        }
    }
}
