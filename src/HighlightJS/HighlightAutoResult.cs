namespace JeremyTCD.WebUtils.SyntaxHighlighters.HighlightJS
{
    public class HighlightAutoResult
    {
        public HighlightAutoResult(double relevance, string value, string language, HighlightAutoResult secondBest)
        {
            Relevance = relevance;
            Value = value;
            Language = language;
            SecondBest = secondBest;
        }

        public double Relevance { get; }
        public string Value { get; }
        public string Language { get; }
        public HighlightAutoResult SecondBest { get; }
    }
}
