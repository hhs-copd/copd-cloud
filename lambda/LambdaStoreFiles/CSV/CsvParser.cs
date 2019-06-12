using System;

namespace LambdaStoreFiles.CSV
{
    public static class CsvParser
    {
        public static IItem Parse(string line)
        {
            var items = line.Split(',');
            if (items.Length != 3)
            {
                throw new ArgumentException("Invalid line");
            }

            var date = DateTimeOffset.Parse(items[0].Trim());
            var sensor = items[1].Trim();
            var value = items[2].Trim();

            switch (sensor)
            {
                case "Temperature":
                    return new TemperatureItem(date, value);
                case "Humidity":
                    return new HumidityItem(date, value);
                case "Heartrate":
                    return new HeartrateItem(date, value);
                case "Thorax circumference":
                    return new ThoraxCircumferenceItem(date, value);
                case "UVA":
                    return new UVAItem(date, value);
                case "UVB":
                    return new UVBItem(date, value);
                case "UVIndex":
                    return new UVIndexItem(date, value);
                case "PM4p0":
                    return new PM4p0Item(date, value);
                case "PM1p0":
                    return new PM1p0Item(date, value);
                case "PM2p5":
                    return new PM2p5Item(date, value);
                case "PM10p0":
                    return new PM10p0Item(date, value);
                case "GPS":
                    return new GPSItem(date, value);
                case "Movement":
                    return new MovementItem(date, value);
                case "IMU":
                    return new IMUItem(date, value);
                default:
                    return new StringItem(date, sensor, value);
            }
        }
    }
}