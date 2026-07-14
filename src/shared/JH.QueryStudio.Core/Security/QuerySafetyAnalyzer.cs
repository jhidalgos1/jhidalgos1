using System.Text.RegularExpressions;

namespace JH.QueryStudio.Core.Security;

public static class QuerySafetyAnalyzer
{
    public static IReadOnlyList<string> Analyze(string sql)
    {
        var normalized = Regex.Replace(sql, @"--.*?$|/\*.*?\*/", string.Empty, RegexOptions.Singleline | RegexOptions.Multiline);
        var risks = new List<string>();

        if (Regex.IsMatch(normalized, @"\bUPDATE\b(?![\s\S]*?\bWHERE\b)", RegexOptions.IgnoreCase))
        {
            risks.Add("UPDATE sin WHERE.");
        }

        if (Regex.IsMatch(normalized, @"\bDELETE\s+FROM\b(?![\s\S]*?\bWHERE\b)", RegexOptions.IgnoreCase))
        {
            risks.Add("DELETE sin WHERE.");
        }

        if (Regex.IsMatch(normalized, @"\bDROP\s+(DATABASE|TABLE)\b|\bTRUNCATE\s+TABLE\b", RegexOptions.IgnoreCase))
        {
            risks.Add("DROP/TRUNCATE detectado.");
        }

        return risks;
    }
}
