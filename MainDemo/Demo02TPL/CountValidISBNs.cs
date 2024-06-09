using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo02TPL
{
    public class CountValidISBNs
    {
        public static int CountValidISBNsSequential(IEnumerable<string> source)
        {
            return source.Where(s => isValidISBN(s)).Count();
        }

        public static int CountValidISBNsParallel(IEnumerable<string> source)
        {
            return source.AsParallel().Where(s => isValidISBN(s)).Count();
        }

        private static bool isValidISBN(string s)
        {
            int digits = 0, check = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '-')
                {
                    if (digits == 0 || digits == 10) return false;
                }
                else if (Char.IsDigit(s[i]) || s[i] == 'X')
                {
                    if (++digits > 10) return false;
                    if (s[i] == 'X' && digits != 10) return false;
                    int val = ((s[i] == 'X') ? 10 : s[i] - '0');
                    check = (check + (11 - digits) * val) % 11;
                }
                else
                {
                    return false;
                }
            }

            return digits == 10 && check == 0;
        }
    }
}
