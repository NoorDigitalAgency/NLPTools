using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Textatistics
{
    public static class Extensions
    {
        private static readonly string[] list = { @"\bm\.m\.", @"\bev\.", @"\bv\.", @"\bkl\.", @"\bresp\.", @"\bleg\.", @"\bmed\.", @"\btel\.", @"\bons\.", @"\bo\.dyl\.", @"\btim\.", @"\bel\.", @"\bnov\.", @"\bmin\.", @"\btis\.", @"\bs\.k\.", @"\bst\.", @"\bs\.", @"\bmån\.", @"\bd\.v\.s\.", @"\bt\.v\.", @"\bkand\.", @"\bjul\.", @"\bkr\.", @"\bdvs\.", @"\bf\.n\.", @"\bd\.ä\.", @"\blör\.", @"\baug\.", @"\bp\.g\.a\.", @"\be\.d\.", @"\bmilj\.", @"\bä\.", @"\bfre\.", @"\bf\.d\.", @"\bokt\.", @"\bang\.", @"\bL\.", @"\bfm\.", @"\bsek\.", @"\bmag\.", @"\bSt\.", @"\btr\.", @"\bdec\.", @"\bem\.", @"\bfeb\.", @"\bsön\.", @"\bjur\.", @"\bkap\.", @"\bv\.g\.", @"\bjan\.", @"\bapr\.", @"\bung\.", @"\bsep\.", @"\bm\.a\.o\.", @"\bo\.d\.", @"\bsal\.", @"\blic\.", @"\btekn\.", @"\bpar\.", @"\bpl\.", @"\bvard\.", @"\bsid\.", @"\be\.dyl\.", @"\bh\.a\.", @"\beg\.", @"\btor\.", @"\bfarm\.", @"\bpol\.", @"\bmar\.", @"\bf\.ö\.", @"\bobs\.", @"\bu\.", @"\bforts\.", @"\bst\.f\.", @"\badr\.", @"\bstud\.", @"\bekon\.", @"\buppl\.", @"\by\.", @"\bmom\.", @"\bjun\.", @"\bd\.y\.", @"\bu\.a\.", @"\bf\.v\.b\.", @"\bteol\.", @"\bäv\.", @"\bu\.ä\.", @"\bskärs\." };

        private static readonly Regex[] regexes = list.Select(s => new Regex(s)).ToArray();

        public static string Code(this string text)
        {
            if (regexes.Any(regex => regex.IsMatch(text)))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);

                return $"hexstring{string.Join("", bytes.Select(b => b.ToString("X2")))}";
            }
            else
            {
                return text;
            }
        }
    }
}