using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NStagger;

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

            new Regex(@"(\w+['""\)\]\%\p{Pf}]*[\u00bf\u00A1?!]+)(\p{Lu}[\w]*[^\.])"), // 8

            new Regex(@"((?:\w*\.\w+)+(?:\.(?![\n]))?)"), // 9

            new Regex(@"(?:^|\s|\.)[-*]\s*(\p{Lu}\w+)"), // 10

            new Regex(@"(?:\r\n|\n|\r)+"), // 11

            new Regex(@"(\w\.)(\p{Lu})"), // 12

            new Regex(@"\b((?:18|19|20)\d{2})\.([0-1]?[0-9])\.([0-1]?[0-9])\b"), // 13

            new Regex(@"\b(?:(?:\d*\.)?(?:\w+\.)+(?:\w+(?:\.\d*)?))"), // 14

            new Regex(@"(?:www\..+?\.\p{L}{2,}|(?:[\w-]*[\w]\.)+(?:com|org|net|se|nu|da|no|fi))"), // 15 

            new Regex(@"\b((?i)(?<!www\.)\w+\.(?-i)N(?i)ET|\w+\.JS)\b"), // 16,

            new Regex(@"\b(?:A|C|J|R|J|X|XBase|Z)\+{1,2}"), // 17,
            
            new Regex(@"\b(?:A|C|F|J|M|Q)#"), // 18
        };

        private static readonly string[] exceptions =
        {
            @"d", @"v", @"ex", @"t", @"A", @"B", @"C", @"D", @"E", @"F", @"G", @"H", @"I", @"J", @"K", @"L", @"M", @"N", @"O", @"P", @"Q", @"R", @"S", @"T", @"U", @"V", @"W", @"X", @"Y", @"Z", @"Ö", @"Ä", @"Å", @"Ø", @"Æ", @"bla", @"fKr",
            @"tex", @"sk", @"etc", @"eKr", @"mm", @"dvs", @"mfl", @"dä", @"dy", @"resp", @"tom", @"kl", @"osv", @"ff", @"eg", @"from", @"Bla", @"gr", @"Tex", @"pga", @"eo", @"ev", @"omkr", @"dr", @"fn", @"Ev", @"From", @"ns", @"alt",
            @"fkr", @"Div", @"e", @"Kl", @"odyl", @"od", @"JM", @"ang", @"ä", @"enl", @"iom", @"Dvs", @"jur", @"tv", @"Sk", @"kk", @"fra", @"MM", @"ca", @"Ex", @"AA", @"w", @"leg", @"k", @"tf", @"Etc", @"ed", @"utg", @"FN", @"ekr", @"kuk",
            @"sp", @"edyl", @"SM", @"em", @"th", @"fl", @"gm", @"Tom", @"mag", @"am", @"sas", @"fö", @"teol", @"mao", @"as", @"sek", @"EM", @"dd", @"m", @"upa", @"Fd", @"FM", @"tr", @"rf", @"SEK", @"dys", @"aka", @"pers", @"blaa", @"Fn",
            @"rok", @"trol", @"farm", @"OD", @"dyl", @"fvt", @"Pga", @"DM", @"tekn", @"SK", @"oa", @"fk", @"ref", @"nhov", @"sa", @"filkand", @"urspr", @"OA", @"frv", @"aa", @"sign", @"AKA", @"stud", @"pol", @"gs", @"vs", @"fom", @"pm",
            @"vd", @"spec", @"med", @"spa", @"mha", @"RAMeissn", @"EKr", @"kv", @"stf", @"Iom", @"krigsv", @"ss", @"SA", @"Pol", @"ua", @"km", @"NWA", @"ba", @"jr", @"fm", @"dv", @"doo", @"den", @"ok", @"ED", @"dsv", @"co", @"starkt",
            @"efterKr", @"dikter", @"phil", @"stundar/", @"polmaster", @"ekon", @"csp", @"civing", @"pgra", @"forts", @"mbpa", @"uvs", @"dreadlocks", @"Aa", @"gatunamn", @"Enl", @"signaturmelodin", @"markeratsa", @"septembergs", @"man",
            @"Farm", @"TOM", @"tsm", @"uta", @"LEGION", @"sdd", @"ie", @"REM", @"syfte", @"pizz", @"jurkand", @"ledarposition", @"fiolmm", @"dag", @"septemberns", @"civek", @"eldyl", @"TDMacfarl", @"sockena", @"polkand", @"sgs", @"philos",
            @"nv", @"glomerulonefrit,sk", @"day", @"igen", @"Guern", @"í-tron", @"libs", @"srl", @"sekr", @"EX", @"om", @"hellom", @"kronan", @"Ekon", @"JParn", @"modemm", @"ffKr", @"ek", @"sjukhus", @"os", @"skk", @"HJ", @"rs", @"pastex",
            @"nshm", @"Polen", @"Tim", @"zinkkarbonata", @"stv", @"berättar/", @"a", @"sm", @"rp", @"solanin", @"ry", @"ism", @"d,vs", @"sv", @"teolkand", @"sta", @"spp", @"nb", @"tillsm", @"DOA", @"intervjuasmm", @"osa", @"rc",
            @"dancehall", @"fv", @"hia", @"alvv", @"betr", @"sn", @"tillhöra"
        };

        private static readonly HashSet<string> hashSet;

        static Extensions()
        {
            hashSet = new HashSet<string>(exceptions);
        }

        public static IEnumerable<string> ToLines(this string text)
        {
            return ToLines(text, false);
        }

        public static IEnumerable<string> ToLines(this string text, bool code)
        {
            text = regexList[10].IsMatch(text) ? regexList[10].Replace(text, "\n$1") : text;

            text = regexList[13].IsMatch(text) ? regexList[13].Replace(text, "$1-$2-$3") : text;

            text = regexList[11].IsMatch(text) ? regexList[11].Replace(text, "\n") : text;

            text = regexList[0].IsMatch(text) ? regexList[0].Replace(text, "$1\n$2") : text;

            text = regexList[1].IsMatch(text) ? regexList[1].Replace(text, "$1\n$2") : text;

            text = regexList[2].IsMatch(text) ? regexList[2].Replace(text, "$1\n$2") : text;

            text = regexList[3].IsMatch(text) ? regexList[3].Replace(text, "$1\n$2") : text;

            text = regexList[8].IsMatch(text) ? regexList[8].Replace(text, "$1\n$2") : text;

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
                else if (regexList[14].IsMatch(words[i]))
                {
                    string word = words[i].Replace(".", "").Trim('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', ' ', '\t', '\r', '\n');

                    if (!hashSet.Contains(word))
                    {
                        if (regexList[12].IsMatch(words[i]) && !regexList[15].IsMatch(words[i]) && !regexList[16].IsMatch(words[i]))
                        {
                            words[i] = regexList[12].Replace(words[i], "$1\n$2");
                        }
                    }
                }

                words[i] = code && regexList[9].IsMatch(words[i]) ? regexList[9].Replace(words[i], m => m.Value.Hex()) : words[i];

                words[i] = code && regexList[17].IsMatch(words[i]) ? regexList[17].Replace(words[i], m => m.Value.Hex()) : words[i];

                words[i] = code && regexList[18].IsMatch(words[i]) ? regexList[18].Replace(words[i], m => m.Value.Hex()) : words[i];

                text += $"{words[i]} ";
            }

            words[i] = code && regexList[17].IsMatch(words[i]) ? regexList[17].Replace(words[i], m => m.Value.Hex()) : words[i];

            words[i] = code && regexList[18].IsMatch(words[i]) ? regexList[18].Replace(words[i], m => m.Value.Hex()) : words[i];

            text += $"{words[i]}";

            text = regexList[7].Replace(text, " ");

            foreach (string line in text.Split("\n", StringSplitOptions.RemoveEmptyEntries))
            {
                string lineToReturn = line.Trim();

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

        public static List<List<NStagger.Token>> TokenizeSentences(this string text)
        {
            List<List<NStagger.Token>> output = new List<List<NStagger.Token>>();

            using (StringReader reader = new StringReader(string.Join("\n", text.ToLines(true))))
            {
                SwedishTokenizer tokenizer = new SwedishTokenizer(reader);

                List<NStagger.Token> tokens;

                while ((tokens = tokenizer.ReadSentence()) != null)
                {
                    output.Add(tokens.Select(token => new NStagger.Token(token.Type, token.Value.UnHex(), token.Offset){IsSpace = token.IsSpace, IsCapitalized = token.IsCapitalized}).ToList());
                }

                return output;
            }
        }
    }
}