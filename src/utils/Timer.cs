using System;
namespace Utils
{
    public static class Timer
    {
        static long current = 0;

        public static void Reset() => current = Now();

        public static long Now() => DateTimeOffset.Now.ToUnixTimeSeconds();
        public static long Stop() {
            long now = Now();
            long res = now - current;
            current = now;
            return res;
        }
    }
}