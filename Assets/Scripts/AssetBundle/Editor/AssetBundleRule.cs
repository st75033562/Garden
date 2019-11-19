using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AssetBundleRuleDatabase
{
    private struct Rule
    {
        public Regex regex;
        public bool inclusion; // false for exclusion
    }

    private readonly List<Rule> m_rules = new List<Rule>();

    private static readonly Regex ReRule = new Regex(@"^\s*(-?)\s*(.+?)\s*$");

    public void Load(string path)
    {
        foreach (var line in File.ReadAllLines(path))
        {
            // ignore comments
            if (line.StartsWith("#"))
            {
                continue;
            }

            var result = ReRule.Match(line);
            if (!result.Success)
            {
                continue;
            }

            m_rules.Add(new Rule {
                regex = new Regex(result.Groups[2].Value),
                inclusion = result.Groups[1].Value != "-"
            });
        }
    }

    public bool Accept(string path)
    {
        bool included = true;
        foreach (var rule in m_rules)
        {
            if (rule.regex.IsMatch(path))
            {
                included = rule.inclusion;
            }
        }
        return included;
    }
}