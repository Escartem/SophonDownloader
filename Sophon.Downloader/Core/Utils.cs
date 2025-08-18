namespace Core
{
    internal class Utils
    {
        private static readonly string[] SizeSuffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
        private const double ScOneSecond = 1000;
        private static long _scLastTick = Environment.TickCount64;
        private static long _scLastReceivedBytes;
        private static double _scLastSpeed;

        public static string FormatSize(double value, int decimalPlaces = 2)
        {
            byte mag = (byte)Math.Log(value, 1000);

            return $"{Math.Round(value / (1L << (mag * 10)), decimalPlaces)} {SizeSuffixes[mag]}";
        }

        public static double CalculateSpeed(long receivedBytes)
        {
            long currentTick = Environment.TickCount64 - _scLastTick + 1;
            long totalReceivedInSecond = Interlocked.Add(ref _scLastReceivedBytes, receivedBytes);
            double speed = totalReceivedInSecond * ScOneSecond / currentTick;

            if (!(currentTick > ScOneSecond))
            {
                return _scLastSpeed;
            }

            _scLastSpeed = speed;
            _ = Interlocked.Exchange(ref _scLastSpeed, speed);
            _ = Interlocked.Exchange(ref _scLastReceivedBytes, 0);
            _ = Interlocked.Exchange(ref _scLastTick, Environment.TickCount64);
            return _scLastSpeed;
        }
    }
}
