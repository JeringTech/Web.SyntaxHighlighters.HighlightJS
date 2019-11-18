using BenchmarkDotNet.Running;

namespace Jering.Web.SyntaxHighlighters.HighlightJS.Performance
{
    public class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
