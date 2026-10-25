using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Bw.Tests.Integration.Architecture.CodeStyle
{
    public static class ParameterColumns
    {
        public const int ConstructorLimit = 6;
        public const int SharedLineLimit = 3;

        private const string Modifiers = "(?:public|private|protected|internal|static|sealed|override|virtual|abstract|readonly|async|unsafe|new|extern|partial)";

        private static readonly Regex Signature = new(@"^\s*(?:\[[^\]]*\]\s*)*(?:" + Modifiers + @"\s+)+[\w<>\[\],.?() ]*?\b(\w+)\s*(?:<[^()]*>)?\s*\(", RegexOptions.Compiled);
        private static readonly Regex Bare = new(@"^\s*(?:void|bool|int|float|string|[A-Z]\w*(?:<[^()]*>)?)\s+([A-Z]\w*)\s*(?:<[^()]*>)?\s*\(", RegexOptions.Compiled);
        private static readonly Regex Statement = new(@"^\s*(?:return|new|var|if|while|for|foreach|switch|using|throw|await|case|else)\b");
        private static readonly Regex TypeName = new(@"\b(?:class|struct|record)\s+(\w+)");
        private static readonly Regex TypedParameter = new(@"\S\s+\S");

        public static (IReadOnlyList<(int Line, string Name, int Count)> SharedLines, IReadOnlyList<(int Line, string Name, int Count)> LongConstructors) Violations(IReadOnlyList<string> lines)
        {
            var shared = new List<(int Line, string Name, int Count)>();
            var constructors = new List<(int Line, string Name, int Count)>();
            foreach (var found in Signatures(lines))
            {
                if (found.Parameters > SharedLineLimit && (found.End == found.Line || found.Rows.Any(count => count > 1) || found.Rows[0] > 0))
                    shared.Add((found.Line + 1, found.Name, found.Parameters));

                if (found.Constructor && found.Parameters > ConstructorLimit)
                    constructors.Add((found.Line + 1, found.Name, found.Parameters));
            }

            return (shared, constructors);
        }

        private static IEnumerable<(int Line, string Name, int Parameters, List<int> Rows, int End, bool Constructor)> Signatures(IReadOnlyList<string> lines)
        {
            var types = new HashSet<string>(TypeName.Matches(string.Join("\n", lines.Select(line => Code(line)))).Cast<Match>().Select(match => match.Groups[1].Value));
            for (var index = 0; index < lines.Count; index++)
            {
                var code = Code(lines[index]);
                if (code.IndexOf('(') < 0 || Statement.IsMatch(code))
                    continue;

                var match = Signature.Match(code);
                if (!match.Success)
                    match = Bare.Match(code);

                if (!match.Success)
                    continue;

                if (!TryReadParameters(lines, index, code.IndexOf('(', match.Index + match.Length - 1), out var parameters, out var rows, out var end))
                    continue;

                if (parameters.Count == 0 || parameters.Any(parameter => !TypedParameter.IsMatch(parameter)))
                    continue;

                var name = match.Groups[1].Value;
                yield return (index, name, parameters.Count, rows, end, types.Contains(name));
            }
        }

        private static bool TryReadParameters(
            IReadOnlyList<string> lines,
            int start,
            int open,
            out List<string> parameters,
            out List<int> rows,
            out int end)
        {
            parameters = new List<string>();
            rows = new List<int>();
            var current = new StringBuilder();
            var depth = 0;
            var column = open;
            for (var row = start; row < lines.Count; row++)
            {
                var code = Code(lines[row]);
                var rowParameters = 0;
                for (; column < code.Length; column++)
                {
                    var symbol = code[column];
                    if (symbol == '(')
                    {
                        depth++;
                        if (depth == 1)
                            continue;
                    }
                    else if (symbol == ')')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            if (current.ToString().Trim().Length > 0)
                            {
                                parameters.Add(current.ToString().Trim());
                                rowParameters++;
                            }

                            rows.Add(rowParameters);
                            end = row;
                            return true;
                        }
                    }

                    if (symbol == ',' && depth == 1)
                    {
                        parameters.Add(current.ToString().Trim());
                        rowParameters++;
                        current.Clear();
                    }
                    else
                    {
                        if (symbol is '<' or '[' or '{')
                            depth++;
                        else if (symbol is '>' or ']' or '}')
                            depth--;

                        current.Append(symbol);
                    }
                }

                rows.Add(rowParameters + (current.ToString().Trim().Length > 0 ? 1 : 0));
                current.Append(' ');
                column = 0;
            }

            end = -1;
            return false;
        }

        private static string Code(string line)
        {
            var quoted = false;
            for (var index = 0; index < line.Length; index++)
            {
                if (line[index] == '"' && (index == 0 || line[index - 1] != '\\'))
                    quoted = !quoted;

                if (!quoted && string.CompareOrdinal(line, index, "//", 0, 2) == 0)
                    return line.Substring(0, index);
            }

            return line;
        }
    }
}
