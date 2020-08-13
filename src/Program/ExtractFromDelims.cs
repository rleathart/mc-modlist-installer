using System.Text.RegularExpressions;

namespace mcmli
{
    partial class Program
    {
        public static MatchCollection ExtractFromDelims(string text, string delims)
        {
            if (delims == "[]") return Regex.Matches(text, @"(?<=\[)([^]]*)(?=\])");
            if (delims == "()") return Regex.Matches(text, @"(?<=\()([^)]*)(?=\))");
            if (delims == "{}") return Regex.Matches(text, @"(?<=\{)([^}]*)(?=\})");

            return Regex.Matches(text,
                @"(?<=" + delims[0] + @")([^" + delims[1] + @"]*)(?=" + delims[1] + @")");
        }
    }
}
