using System.Diagnostics;
using System.Text;

namespace SimplestDemo
{
    public static class EnumerableExtensions
    {
        public static double Median(this IEnumerable<long> source)
        {
            if (source == null || !source.Any())
            {
                throw new ArgumentException("Cannot compute median for a null or empty set.");
            }

            var sortedList = source.OrderBy(num => num).ToList();
            int itemIndex = sortedList.Count / 2;

            if (sortedList.Count % 2 == 0)
            {
                // If even, average the two middle elements
                return (sortedList[itemIndex] + sortedList[itemIndex - 1]) / 2;
            }
            else
            {
                // If odd, return the middle element
                return sortedList[itemIndex];
            }
        }
    }

    public class RunStatistics
    {
        List<long> _runs = new List<long>();
        private readonly string _title;
        private readonly string _subtitle;

        public RunStatistics(string title, string subtitle) { _title = title; _subtitle = subtitle; }
        public void AddRun(long ms)
        {
            _runs.Add(ms);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"-=-= {_title} -=-=");
            sb.AppendLine($"Runs:{_subtitle}");
            //_runs.ForEach(a => sb.AppendLine(a.ToString()));
            sb.AppendLine($"Cnt:{_runs.Count()}");
            sb.AppendLine($"Average:{_runs.Average(a => a)}");
            sb.AppendLine($"Min:{_runs.Min()}");
            sb.AppendLine($"Max:{_runs.Max()}");
            sb.AppendLine($"Median:{_runs.Median()}");
            return sb.ToString();
        }
    }
    public class SimpleBenchmarkRun
    {
        private readonly string _title;

        /// <summary>
        /// Number of iteration
        /// </summary>
        public int N { get; set; } = 4;
        /// <summary>
        /// Iterations to skip
        /// </summary>
        public int Skip { get; set; } = 1;

        public SimpleBenchmarkRun(string title, int n = 4)
        {
            _title = title;
            N = n;
        }
        public RunStatistics Run(Action torun, string subtitle = "")
        {
            RunStatistics runStatistics = new RunStatistics(_title, subtitle);
            Stopwatch stopwatch = new Stopwatch();
            long lastms = 0;
            for (int i = 0; i < Skip; i++)
            {
                torun();
            }
            stopwatch.Start();
            for (int i = 0; i < N; i++)
            {
                torun();
                runStatistics.AddRun(stopwatch.ElapsedMilliseconds - lastms);
                lastms = stopwatch.ElapsedMilliseconds;
            }
            stopwatch.Stop();
            return runStatistics;
        }
        public async Task<RunStatistics> RunAsync(Func<Task> torun, string subtitle = "")
        {
            RunStatistics runStatistics = new RunStatistics(_title, subtitle);
            Stopwatch stopwatch = new Stopwatch();
            long lastms = 0;
            for (int i = 0; i < Skip; i++)
            {
                await torun();
            }
            stopwatch.Start();
            for (int i = 0; i < N; i++)
            {
                await torun();
                runStatistics.AddRun(stopwatch.ElapsedMilliseconds - lastms);
                lastms = stopwatch.ElapsedMilliseconds;
            }
            stopwatch.Stop();
            return runStatistics;
        }
    }
}
