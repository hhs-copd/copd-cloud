using System;

namespace LambdaStoreFiles.CSV
{
    public class TemperatureCSVItem : ICSVItem
    {
        public DateTimeOffset DateTime { get; }

        public float Value { get; }

        public TemperatureCSVItem(DateTimeOffset date, string sensor, string value)
        {
            this.DateTime = date;
            this.Value = float.Parse(value);
        }
    }
}
