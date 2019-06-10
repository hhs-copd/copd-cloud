using System;

namespace LambdaStoreFiles.CSV
{
    internal class HumidityCSVItem : ICSVItem
    {
        public DateTimeOffset DateTime { get; }

        public float Value { get; }

        public HumidityCSVItem(DateTimeOffset date, string sensor, string value)
        {
            this.DateTime = date;
            this.Value = float.Parse(value);
        }
    }
}
