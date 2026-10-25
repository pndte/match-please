using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bw.Tests.Integration.Architecture.CodeStyle
{
    public static class BlockSpacing
    {
        private const int ShownLength = 90;

        private static readonly Regex Header = new(@"^(if|for|foreach|while|switch)\s*\(|^else\b");
        private static readonly Regex LineComment = new(@"//.*$");
        private static readonly Regex StringLiteral = new(@"""(?:\\.|[^""\\])*""");
        private static readonly string[] AllowedAfterABlock = { "}", "else", "catch", "finally", "case ", "default:", ")", "]" };

        public static IReadOnlyList<(int Line, string Text)> Violations(IReadOnlyList<string> lines)
        {
            var found = new SortedSet<(int Line, string Text)>();
            for (var index = 0; index < lines.Count; index++)
            {
                var stripped = lines[index].Trim();
                if (!Header.IsMatch(stripped) || Starts(stripped, "else"))
                    continue;

                if (index > 0 && Starts(lines[index - 1].Trim(), "else"))
                    continue;

                var end = BlockEnd(lines, index);
                if (end + 1 >= lines.Count)
                    continue;

                var after = lines[end + 1].Trim();
                if (after.Length > 0 && !AllowedAfterABlock.Any(allowed => Starts(after, allowed)))
                    found.Add((end + 2, after.Length > ShownLength ? after.Substring(0, ShownLength) : after));
            }

            return found.ToList();
        }

        private static int BlockEnd(IReadOnlyList<string> lines, int index)
        {
            var stripped = lines[index].Trim();
            int headerEnd;
            string rest;
            if (Starts(stripped, "else") && !Starts(stripped, "else if"))
            {
                headerEnd = index;
                var code = Code(stripped);
                rest = code.Length > 4 ? code.Substring(4).Trim() : "";
            }
            else
            {
                (headerEnd, rest) = BalancedHeaderEnd(lines, index);
            }

            if (rest.Length > 0 && rest != "{")
                return FollowElse(lines, Starts(rest, "{") ? MatchingBrace(lines, headerEnd) : InlineStatementEnd(lines, headerEnd));

            var next = NextNonBlank(lines, headerEnd + 1);
            if (next < 0)
                return headerEnd;

            return FollowElse(lines, lines[next].Trim() == "{" ? MatchingBrace(lines, next) : StatementEnd(lines, next));
        }

        private static (int Line, string After) BalancedHeaderEnd(IReadOnlyList<string> lines, int index)
        {
            var depth = 0;
            var seen = false;
            for (var line = index; line < lines.Count; line++)
            {
                var text = Code(lines[line]);
                for (var position = 0; position < text.Length; position++)
                {
                    if (text[position] == '(')
                    {
                        depth++;
                        seen = true;
                    }
                    else if (text[position] == ')')
                    {
                        depth--;
                        if (seen && depth == 0)
                            return (line, text.Substring(position + 1).Trim());
                    }
                }
            }

            return (lines.Count - 1, "");
        }

        private static int StatementEnd(IReadOnlyList<string> lines, int index)
        {
            if (Header.IsMatch(lines[index].Trim()))
                return BlockEnd(lines, index);

            var depth = 0;
            for (var line = index; line < lines.Count; line++)
            {
                var text = Code(lines[line]);
                depth += Count(text, '{') - Count(text, '}') + Count(text, '(') - Count(text, ')');
                if (depth <= 0 && text.TrimEnd().EndsWith(";", StringComparison.Ordinal))
                    return line;
            }

            return lines.Count - 1;
        }

        private static int InlineStatementEnd(IReadOnlyList<string> lines, int index)
        {
            var depth = 0;
            for (var line = index; line < lines.Count; line++)
            {
                var text = Code(lines[line]);
                depth += Count(text, '{') - Count(text, '}');
                var trimmed = text.TrimEnd();
                if (depth <= 0 && (trimmed.EndsWith(";", StringComparison.Ordinal) || trimmed.EndsWith("}", StringComparison.Ordinal)))
                    return line;
            }

            return lines.Count - 1;
        }

        private static int MatchingBrace(IReadOnlyList<string> lines, int index)
        {
            var depth = 0;
            for (var line = index; line < lines.Count; line++)
            {
                var text = Code(lines[line]);
                depth += Count(text, '{') - Count(text, '}');
                if (depth <= 0)
                    return line;
            }

            return lines.Count - 1;
        }

        private static int FollowElse(IReadOnlyList<string> lines, int end)
        {
            var next = NextNonBlank(lines, end + 1);
            return next == end + 1 && Starts(lines[next].Trim(), "else") ? BlockEnd(lines, next) : end;
        }

        private static int NextNonBlank(IReadOnlyList<string> lines, int index)
        {
            for (var line = index; line < lines.Count; line++)
                if (lines[line].Trim().Length > 0)
                    return line;

            return -1;
        }

        private static string Code(string text) =>
            StringLiteral.Replace(LineComment.Replace(text, ""), "\"\"");

        private static int Count(string text, char symbol) =>
            text.Count(character => character == symbol);

        private static bool Starts(string text, string prefix) =>
            text.StartsWith(prefix, StringComparison.Ordinal);
    }
}
