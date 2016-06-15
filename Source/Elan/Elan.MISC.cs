using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

//version 2016.06.11
namespace Elan.MISC
{
    public static class MISC
    {
        public static string GetPathable_YMD(DateTime date_time)
        {
            return $"{date_time.Year}{date_time.Month.ToString().PadLeft(2, '0')}{date_time.Day.ToString().PadLeft(2, '0')}";
        }
        public static string GetPathable_HM(DateTime date_time)
        {
            return $"{date_time.Hour.ToString().PadLeft(2, '0')}{ date_time.Minute.ToString().PadLeft(2, '0')}";
        }
        public static string GetPathable_TMDHM(DateTime date_time)
        {
            return $"{GetPathable_YMD(date_time)}{GetPathable_HM(date_time)}";
        }
        public static string GetSpaceString_ofLength(int num)
        {
            if (num < 1)
            {
                return String.Empty;
            }
            else
            {
                var sb = new StringBuilder();
                for (int j = 0; j < num; j++)
                {
                    sb.Append(" ");
                }
                return sb.ToString();
            }
        }

        public static long TryGet_FileLength(string file_path)
        {
            long _R;
            try
            {
                _R = (new FileInfo(file_path)).Length;
            }
            catch
            {
                _R = -1;
            }
            return _R;
        }

        public static string GetString_ofFileLength(string file_path)
        {
            long size = TryGet_FileLength(file_path);
            if (size > 0)
            {
                return $"{String.Format("{0:N0}", size)} bytes";
            }
            else
            {
                return "fail to get file size";
            }
        }
    }

    public static class Print_toConsole_Ex
    {
        public static void Print_toConsole<T>(this IEnumerable<T> input)
        {
            var sb = new StringBuilder();
            int c = 0;
            foreach (var v in input)
            {
                sb.AppendLine($"[{c}] = {v.ToString()}");
                c++;
            }
            Console.WriteLine(sb.ToString());
        }
        public static void PrintDict_toConsole<TKey, TValue>(this IDictionary<TKey, TValue> input)
        {
            var sb = new StringBuilder();
            foreach (var v in input)
            {
                sb.AppendLine($"[{v.Key.ToString()}] = {v.Value.ToString()}");
            }
            Console.WriteLine(sb.ToString());
        }
    }

    class RangeTest<T> where T : IComparable<T>
    {
        int sample_count = 0;
        string Name;
        T Upper;
        T Lower;
        T lastMidVal;
        int MidValSetTime = 0;
        public RangeTest(string name)
        {
            Name = name;
        }
        public void Feed(T v)
        {
            sample_count += 1;
            if (sample_count == 1)
            {
                Upper = v;
            }
            else if (sample_count == 2)
            {
                if (v.CompareTo(Upper) > 0)
                {
                    Lower = Upper;
                    Upper = v;
                }
                else
                {
                    Lower = v;
                }
            }
            else
            {
                if (v.CompareTo(Upper) > 0) Upper = v;
                else if (v.CompareTo(Lower) < 0) Lower = v;
                else
                {
                    lastMidVal = v;
                    MidValSetTime += 1;
                }
            }
        }
        public void PrintResult()
        {
            Console.WriteLine($"Range of \"{Name}\": [{Lower}, {Upper}]");
            Console.WriteLine($"  Range break times: {sample_count - MidValSetTime - 2}");
            if (MidValSetTime > 0) Console.WriteLine($"  Value of the last collision: {lastMidVal.ToString()}");
            Console.WriteLine($"  Sample count: {sample_count}");
            Console.WriteLine();
        }
    }

    class TimeTest
    {
        Action func;
        string name;
        public TimeTest(Action action, string test_name)
        {
            func = action; name = test_name;
        }
        public void Run()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            func();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine($"Result of Time Test \"{name}\": {ts.TotalMilliseconds} ms.");
        }
    }

}
