using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Textatistics
{
    public static class Extensions
    {
        private static readonly Regex[] regexList =
        {
            new Regex(@"([?!]) +(['""([\u00bf\u00A1\p{Pi}]*[\p{Lu}])"), // 0

            new Regex(@"(\.[\.]+) +(['""([\u00bf\u00A1\p{Pi}]*[\p{Lu}])"), // 1

            new Regex(@"([?!\.][\ ]*['"")\]\p{Pf}]+) +(['""([\u00bf\u00A1\p{Pi}]*[\ ]*[\p{Lu}])"), // 2

            new Regex(@"([?!\.]) +(['""([\u00bf\u00A1\p{Pi}]+[\ ]*[\p{Lu}])"), // 3

            new Regex(@"([\p{L}\p{Nl}\p{Nd}\.\-]*)([\'\""\)\]\%\p{Pf}]*)(\.+)$"), // 4

            new Regex(@"(?:\.)[\p{Lu}\-]+(?:\.+)$"), // 5

            new Regex(@"^(?:[ ]*['""([\u00bf\u00A1\p{Pi}]*[ ]*[\p{Lu}0-9])"), // 6

            new Regex(" +"), // 7

            new Regex(@"(\w+['""\)\]\%\p{Pf}]*[\u00bf\u00A1?!\.]+)(\p{Lu}[\w]*[^\.])"), // 8

            new Regex(@"(?:^|\s|-)(?:((?:\w+\.){2,})(?:\.*)$|((?:\w+\.)+)(?!$)[^\p{L}])"), // 9

            new Regex(@" [-*] (\p{Lu}\w+\s)"), // 10

            new Regex(@"(?:\r\n|\n|\r)+"), // 11

            new Regex(@"^\s*(\d+)\s*\.\s+(\p{Lu})"), // 12

            new Regex(@"\b((?:18|19|20)\d{2})\.([0-1]?[0-9])\.([0-1]?[0-9])\b") // 13
        };

        public static IEnumerable<string> ToLines(this string text, bool code)
        {
            text = regexList[11].IsMatch(text) ? regexList[11].Replace(text, "\n") : text;

            text = regexList[0].IsMatch(text) ? regexList[0].Replace(text, "$1\n$2") : text;

            text = regexList[1].IsMatch(text) ? regexList[1].Replace(text, "$1\n$2") : text;

            text = regexList[2].IsMatch(text) ? regexList[2].Replace(text, "$1\n$2") : text;

            text = regexList[3].IsMatch(text) ? regexList[3].Replace(text, "$1\n$2") : text;

            text = regexList[8].IsMatch(text) ? regexList[8].Replace(text, "$1\n$2") : text;

            text = regexList[10].IsMatch(text) ? regexList[10].Replace(text, "\n$1") : text;

            text = regexList[12].IsMatch(text) ? regexList[12].Replace(text, "$2") : text;

            text = regexList[13].IsMatch(text) ? regexList[13].Replace(text, "$1-$2-$3") : text;

            string[] words = regexList[7].Split(text);

            text = "";

            int i;

            for (i = 0; i < words.Length - 1; i++)
            {
                Match match = regexList[4].Match(words[i]);

                if (match.Success)
                {
                    string prefix = match.Groups[0].Success ? match.Groups[0].Value : null;

                    string startingPunctuation = match.Groups[1].Success ? match.Groups[1].Value : null;

                    if (prefix != null && startingPunctuation == null)
                    {
                    }
                    else if (regexList[5].IsMatch(words[i]))
                    {
                    }
                    else if (regexList[6].IsMatch(words[i + 1]))
                    {
                        words[i] += "\n";
                    }
                }

                text += $"{words[i]} ";
            }

            text += $"{words[i]}";

            text = regexList[7].Replace(text, " ");

            foreach (string line in text.Split("\n", StringSplitOptions.RemoveEmptyEntries))
            {
                string lineToReturn = line.Trim();

                while (code && regexList[9].IsMatch(lineToReturn))
                {
                    Match match = regexList[9].Match(lineToReturn);

                    bool lineEnd = match.Groups[2].Success;

                    Group matchGroup = lineEnd ? match.Groups[2] : match.Groups[1];

                    int index = matchGroup.Index;

                    int length = matchGroup.Length;

                    string before = lineToReturn.Substring(0, index);

                    string after = lineToReturn.Substring(index + length);

                    string hexString = matchGroup.Value.Hex();

                    lineToReturn = $"{before}{hexString}{after}";
                }

                if (!string.IsNullOrWhiteSpace(lineToReturn))
                {
                    yield return lineToReturn;
                }
            }
        }

        private static readonly Regex unHexRegex = new Regex(@"\bhexstring[A-F0-9]+x");

        public static IEnumerable<string> UnHex(this IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                string l = line;

                if (unHexRegex.IsMatch(l))
                {
                    l = unHexRegex.Replace(l, match =>
                    {
                        string hex = match.Value.Substring(9, match.Length - 10);

                        return Encoding.UTF8.GetString(Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray());
                    });
                }

                yield return l;
            }
        }

        public static string UnHex(this string text)
        {
            return UnHex(new[] { text }).First();
        }

        public static string Hex(this string text)
        {
            return $"hexstring{string.Join("", Encoding.UTF8.GetBytes(text).Select(b => b.ToString("X2")))}x";
        }
    }
}