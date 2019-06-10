using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Textatistics
{
    public static class Extensions
    {
        private static readonly string[] list =
        {
            "m.m.", "ev.", "v.", "kl.", "resp.", "leg.", "med.", "tel.", "ons.", "o.dyl.", "tim.", "el.", "nov.", "min.", "tis.", "s.k.", "st.", "s.", "mån.", "d.v.s.", "t.v.", "kand.", "jul.", "kr.", "dvs.", "f.n.", "d.ä.", "lör.", "aug.",

            "p.g.a.", "e.d.", "milj.", "ä.", "fre.", "f.d.", "okt.", "ang.", "L.", "fm.", "sek.", "mag.", "St.", "tr.", "dec.", "em.", "feb.", "sön.", "jur.", "kap.", "v.g.", "jan.", "apr.", "ung.", "sep.", "m.a.o.", "o.d.", "sal.", "lic.",

            "tekn.", "par.", "pl.", "vard.", "sid.", "e.dyl.", "h.a.", "eg.", "tor.", "farm.", "pol.", "mar.", "f.ö.", "obs.", "u.", "forts.", "st.f.", "adr.", "stud.", "ekon.", "uppl.", "y.", "mom.", "jun.", "d.y.", "u.a.", "f.v.b.",

            "teol.", "äv.", "u.ä.", "skärs."
        };

        private static readonly HashSet<string> hashSet;

        private static readonly Regex[] regexList =
        {
            new Regex(@"([?!]) +(['""([\u00bf\u00A1\p{Pi}]*[\p{Lu}])"), new Regex(@"(\.[\.]+) +(['""([\u00bf\u00A1\p{Pi}]*[\p{Lu}])"), new Regex(@"([?!\.][\ ]*['"")\]\p{Pf}]+) +(['""([\u00bf\u00A1\p{Pi}]*[\ ]*[\p{Lu}])"),

            new Regex(@"([?!\.]) +(['""([\u00bf\u00A1\p{Pi}]+[\ ]*[\p{Lu}])"), new Regex(@"([\p{L}\p{Nl}\p{Nd}\.\-]*)([\'\""\)\]\%\p{Pf}]*)(\.+)$"), new Regex(@"(?:\.)[\p{Lu}\-]+(?:\.+)$"),

            new Regex(@"^(?:[ ]*['""([\u00bf\u00A1\p{Pi}]*[ ]*[\p{Lu}0-9])"), new Regex(" +")
        };

        private static readonly Dictionary<Regex, string> codes;

        private static readonly Dictionary<string, string> decodes;

        static Extensions()
        {
            hashSet = new HashSet<string>(list);

            codes = list.ToDictionary(s => new Regex($@"\b{s.Replace(".", @"\.")}"), s => $"hexstring{string.Join("", Encoding.UTF8.GetBytes(s).Select(b => b.ToString("X2")))}");

            decodes = list.ToDictionary(s => $"hexstring{string.Join("", Encoding.UTF8.GetBytes(s).Select(b => b.ToString("X2")))}", s => s);
        }

        public static IEnumerable<string> ToLines(this string text, bool code)
        {
            text = regexList[0].IsMatch(text) ? regexList[0].Replace(text, "$1\n$2") : text;

            text = regexList[1].IsMatch(text) ? regexList[1].Replace(text, "$1\n$2") : text;

            text = regexList[2].IsMatch(text) ? regexList[2].Replace(text, "$1\n$2") : text;

            text = regexList[3].IsMatch(text) ? regexList[3].Replace(text, "$1\n$2") : text;

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

                    if (prefix != null && hashSet.Contains(prefix) && startingPunctuation == null)
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

            foreach ((Regex key, var value) in codes)
            {
                text = key.Replace(text, value);
            }

            foreach (string line in text.Split("\n", StringSplitOptions.RemoveEmptyEntries))
            {
                yield return line.Trim();
            }
        }

        public static IEnumerable<string> ToLines(this string text)
        {
            return ToLines(text, false);
        }

        public static IEnumerable<string> UnHex(this IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                string l = line;

                foreach (string hexString in decodes.Keys)
                {
                    l = l.Replace(hexString, decodes[hexString]);
                }

                yield return l;
            }
        }

        public static string UnHex(this string text)
        {
            return UnHex(new[] { text }).First();
        }
    }
}