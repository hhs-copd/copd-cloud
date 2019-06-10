using System;
using System.Linq;

namespace LambdaStoreFiles.CSV
{
    internal class GPSCSVItem : ICSVItem
    {
        public DateTimeOffset DateTime { get; }

        public float[] Value { get; }

        public GPSCSVItem(DateTimeOffset date, string sensor, string value)
        {
            this.DateTime = date;
            this.Value = value.Split(";").Select(float.Parse).ToArray();
        }
    }
}
