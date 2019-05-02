using System;

namespace LambdaUserStore
{
    public sealed class DataEntry
    {
        /*
         * Anonymous User Identifier from Auth0
         */
        public string UserId { get; set; }

        /*
         * Time of measuring
         */
        public DateTimeOffset TimeStamp { get; set; }

        /*
         * Beats per minute
         */
        public int? HeartRate { get; set; }

        /*
         * Inhales per minute
         */
        public int? BreathRate { get; set; }

        /*
         * Liters of air inhaled per minute
         */
        public float? AirIntake { get; set; }

        /*
         * Particles in the air in PM2.5 μg/m3
         */
        public float? AirQualityPM2p5 { get; set; }

        /*
         * Particles in the air in PM10 μg/m3
         */
        public float? AirQualityPM10 { get; set; }

        /*
         * Temperature in Celsius
         */
        public float? Temperature { get; set; }

        /*
         * Humidity
         */
        public float? Humidity { get; set; }

        /*
         * UV-B Light
         */
        public int? UV { get; set; }

        /*
         * Acceleration on X axis
         */
        public int? AccelerationX { get; set; }

        /*
         * Acceleration on Y axis
         */
        public int? AccelerationY { get; set; }

        /*
         * Acceleration on Z axis
         */
        public int? AccelerationZ { get; set; }

        /*
         * Gyroscope on X axis
         */
        public int? GyroX { get; set; }

        /*
         * Gyroscope on Y axis
         */
        public int? GyroY { get; set; }

        /*
         * Gyroscope on Z axis
         */
        public int? GyroZ { get; set; }

        /*
         * Latitude from GPS
         */
        public float? CoordinateLatitude { get; set; }

        /*
         * Longitude from GPS
         */
        public float? CoordinateLongitude { get; set; }
    }
}