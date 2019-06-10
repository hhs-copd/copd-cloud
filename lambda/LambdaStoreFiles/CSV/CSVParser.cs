using System;

namespace LambdaStoreFiles.CSV
{
    public sealed class CSVParser
    {
        public ICSVItem Parse(string line)
        {
            string[] items = line.Split(',');
            if (items.Length != 3)
            {
                throw new ArgumentException("Invalid line");
            }

            DateTimeOffset date = DateTimeOffset.Parse(items[0]);
            string sensor = items[1];
            string value = items[2];

            switch (sensor)
            {
                case "Temperature":
                    return new TemperatureCSVItem(date, sensor, value);
                case "Humidity":
                    return new HumidityCSVItem(date, sensor, value);
                case "Heartrate":
                    return new HeartrateCSVItem(date, sensor, value);
                case "GPS":
                    return new GPSCSVItem(date, sensor, value);
                case "UV":
                    return new UVCSVItem(date, sensor, value);
                case "Movement":
                    return new MovementCSVItem(date, sensor, value);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
