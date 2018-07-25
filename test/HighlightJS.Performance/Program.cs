using BenchmarkDotNet.Running;

namespace Jering.WebUtils.SyntaxHighlighters.HighlightJS.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
