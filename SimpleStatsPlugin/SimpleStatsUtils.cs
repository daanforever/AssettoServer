using DotNext;

namespace SimpleStatsPlugin
{
    public static class Utils
    {
        public static string LapTimeFormat(uint lapTime)
        {
            //var lt = lapTime.ToString("D7");
            //return $"{lt.Substring(0, 2)}:{lt.Substring(2, 2)}.{lt.Substring(4, 3)}";
            return DateTime.UnixEpoch.AddMilliseconds(lapTime).ToString("mm:ss.fff");
        }
    }
}
