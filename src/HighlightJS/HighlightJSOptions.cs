using System.Collections.Generic;

namespace JeremyTCD.WebUtils.SyntaxHighlighters.HighlightJS
{
    public class HighlightJSOptions
    {
        public bool IgnoreIllegals { get; set; } = true;
        public bool TabReplace { get; set; }
        public string ClassPrefix { get; set; } = "hljs-";
        public string[] LanguageSubset { get; set; }
    }
}
