using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WorkAttend.Shared.Extensions
{
    public static class ExtensionMethods
    {
        private const string KEY = "DKR@POSK";
        private const string IV = "DKR@POSV";
        public static string generateRandomHashString(this string hash)
        {
            byte[] guidByte = Guid.NewGuid().ToByteArray();
            byte[] user = Encoding.ASCII.GetBytes(hash);
            byte[] date = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] concat = new byte[guidByte.Length + user.Length + date.Length];
            System.Buffer.BlockCopy(guidByte, 0, concat, 0, guidByte.Length);
            System.Buffer.BlockCopy(user, 0, concat, guidByte.Length - 1, user.Length);
            System.Buffer.BlockCopy(date, 0, concat, user.Length - 1, date.Length);
            return Convert.ToBase64String(concat);
        }
        public static StringBuilder TextToHTML(this StringBuilder HTMLFilePath, string FilePath)
        {
            System.Text.StringBuilder storeContent = new System.Text.StringBuilder();

            try
            {

                using (System.IO.StreamReader htmlReader = new System.IO.StreamReader(FilePath.ToString()))
                {
                    string lineStr;
                    while ((lineStr = htmlReader.ReadLine()) != null)
                    {
                        storeContent.Append(lineStr);
                    }
                }
            }
            catch (Exception objError)
            {
                throw objError;
            }

            return storeContent;
        }

        public static string Decrypt(this string encText)
        {
            if (encText.Trim().Length == 0)
                return string.Empty;
            byte[] bKey, bIV, bInput;

            bKey = System.Text.Encoding.UTF8.GetBytes(KEY);
            bIV = System.Text.Encoding.UTF8.GetBytes(IV);
            bInput = Convert.FromBase64String(encText);

            MemoryStream memStream = new MemoryStream();

            DES des = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(memStream, des.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write);

            encStream.Write(bInput, 0, bInput.Length);
            encStream.FlushFinalBlock();

            return (System.Text.Encoding.UTF8.GetString(memStream.ToArray()));
        }

        public static string Encrypt(this string text)
        {
            if (text.Trim().Length == 0)
                return string.Empty;
            byte[] bKey, bIV, bInput;

            bKey = System.Text.Encoding.UTF8.GetBytes(KEY);
            bIV = System.Text.Encoding.UTF8.GetBytes(IV);
            bInput = System.Text.Encoding.UTF8.GetBytes(text);

            MemoryStream memStream = new MemoryStream();

            DES des = new DESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(memStream, des.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write);

            encStream.Write(bInput, 0, bInput.Length);
            encStream.FlushFinalBlock();
            string st = Convert.ToBase64String(memStream.ToArray());

            return (st);
        }

        public static string ToMD5Hash(this string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string Wordify(this string value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }
        public static string Sentencify(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return ToTitleCase(value);
        }

        #region Singular and Plural
        private static readonly List<InflectorRule> _plurals = new List<InflectorRule>();
        private static readonly List<InflectorRule> _singulars = new List<InflectorRule>();
        private static readonly List<string> _uncountables = new List<string>();
        static ExtensionMethods()
        {
            AddPluralRule("$", "s");
            AddPluralRule("s$", "s");
            AddPluralRule("(ax|test)is$", "$1es");
            AddPluralRule("(octop|vir)us$", "$1i");
            AddPluralRule("(alias|status)$", "$1es");
            AddPluralRule("(bu)s$", "$1ses");
            AddPluralRule("(buffal|tomat)o$", "$1oes");
            AddPluralRule("([ti])um$", "$1a");
            AddPluralRule("sis$", "ses");
            AddPluralRule("(?:([^f])fe|([lr])f)$", "$1$2ves");
            AddPluralRule("(hive)$", "$1s");
            AddPluralRule("([^aeiouy]|qu)y$", "$1ies");
            AddPluralRule("(x|ch|ss|sh)$", "$1es");
            AddPluralRule("(matr|vert|ind)ix|ex$", "$1ices");
            AddPluralRule("([m|l])ouse$", "$1ice");
            AddPluralRule("^(ox)$", "$1en");
            AddPluralRule("(quiz)$", "$1zes");

            AddSingularRule("s$", String.Empty);
            AddSingularRule("ss$", "ss");
            AddSingularRule("(n)ews$", "$1ews");
            AddSingularRule("([ti])a$", "$1um");
            AddSingularRule("((a)naly|(b)a|(d)iagno|(p)arenthe|(p)rogno|(s)ynop|(t)he)ses$", "$1$2sis");
            AddSingularRule("(^analy)ses$", "$1sis");
            AddSingularRule("([^f])ves$", "$1fe");
            AddSingularRule("(hive)s$", "$1");
            AddSingularRule("(tive)s$", "$1");
            AddSingularRule("([lr])ves$", "$1f");
            AddSingularRule("([^aeiouy]|qu)ies$", "$1y");
            AddSingularRule("(s)eries$", "$1eries");
            AddSingularRule("(m)ovies$", "$1ovie");
            AddSingularRule("(x|ch|ss|sh)es$", "$1");
            AddSingularRule("([m|l])ice$", "$1ouse");
            AddSingularRule("(bus)es$", "$1");
            AddSingularRule("(o)es$", "$1");
            AddSingularRule("(shoe)s$", "$1");
            AddSingularRule("(cris|ax|test)es$", "$1is");
            AddSingularRule("(octop|vir)i$", "$1us");
            AddSingularRule("(alias|status)$", "$1");
            AddSingularRule("(alias|status)es$", "$1");
            AddSingularRule("^(ox)en", "$1");
            AddSingularRule("(vert|ind)ices$", "$1ex");
            AddSingularRule("(matr)ices$", "$1ix");
            AddSingularRule("(quiz)zes$", "$1");

            AddIrregularRule("person", "people");
            AddIrregularRule("man", "men");
            AddIrregularRule("child", "children");
            AddIrregularRule("sex", "sexes");
            AddIrregularRule("tax", "taxes");
            AddIrregularRule("move", "moves");
            AddUnknownCountRule("equipment");
            AddUnknownCountRule("information");
            AddUnknownCountRule("rice");
            AddUnknownCountRule("money");
            AddUnknownCountRule("species");
            AddUnknownCountRule("series");
            AddUnknownCountRule("fish");
            AddUnknownCountRule("sheep");

        }
        public static string MakeInitialCaps(string word)
        {
            return String.Concat(word.Substring(0, 1).ToUpper(), word.Substring(1).ToLower());
        }
        private static void AddUnknownCountRule(string word)
        {
            _uncountables.Add(word.ToLower());
        }
        private static void AddPluralRule(string rule, string replacement)
        {
            _plurals.Add(new InflectorRule(rule, replacement));
        }
        public static string MakeSingular(string word)
        {
            return ApplyRules(_singulars, word);
        }
        private static string ApplyRules(IList<InflectorRule> rules, string word)
        {
            string result = word;
            if (!_uncountables.Contains(word.ToLower()))
            {
                for (int i = rules.Count - 1; i >= 0; i--)
                {
                    string currentPass = rules[i].Apply(word);
                    if (currentPass != null)
                    {
                        result = currentPass;
                        break;
                    }
                }
            }
            return result;
        }
        public static string ToHumanCase(string lowercaseAndUnderscoredWord)
        {
            return MakeInitialCaps(Regex.Replace(lowercaseAndUnderscoredWord, @"_", " "));
        }
        public static string AddUnderscores(string pascalCasedWord)
        {
            return Regex.Replace(Regex.Replace(Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])", "$1_$2"), @"[-\s]", "_").ToLower();
        }
        public static string ToTitleCase(string word)
        {
            return Regex.Replace(ToHumanCase(AddUnderscores(word)), @"\b([a-z])",
                delegate (Match match) { return match.Captures[0].Value.ToUpper(); }).Replace("'S", "'s");
        }
        /// <summary>
        /// Adds the singular rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="replacement">The replacement.</param>
        private static void AddSingularRule(string rule, string replacement)
        {
            _singulars.Add(new InflectorRule(rule, replacement));
        }
        private static void AddIrregularRule(string singular, string plural)
        {
            AddPluralRule(String.Concat("(", singular[0], ")", singular.Substring(1), "$"), String.Concat("$1", plural.Substring(1)));
            AddSingularRule(String.Concat("(", plural[0], ")", plural.Substring(1), "$"), String.Concat("$1", singular.Substring(1)));
        }



        private class InflectorRule
        {
            /// <summary>
            /// 
            /// </summary>
            public readonly Regex regex;

            /// <summary>
            /// 
            /// </summary>
            public readonly string replacement;

            /// <summary>
            /// Initializes a new instance of the <see cref="InflectorRule"/> class.
            /// </summary>
            /// <param name="regexPattern">The regex pattern.</param>
            /// <param name="replacementText">The replacement text.</param>
            public InflectorRule(string regexPattern, string replacementText)
            {
                regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
                replacement = replacementText;
            }

            /// <summary>
            /// Applies the specified word.
            /// </summary>
            /// <param name="word">The word.</param>
            /// <returns></returns>
            public string Apply(string word)
            {
                if (!regex.IsMatch(word))
                    return null;

                string replace = regex.Replace(word, replacement);
                if (word == word.ToUpper())
                    replace = replace.ToUpper();

                return replace;
            }
        }
        #endregion

        public static string IfNullOrWhiteSpaces(this string value, string emptycharacter = "-")
        {
            return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value) ? emptycharacter : value;
        }
        public static byte[] Zip(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(this byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, System.IO.Compression.CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return ASCIIEncoding.UTF8.GetString(mso.ToArray());
            }


        }

        public static string Singlofy(this string value)
        {
            return MakeSingular(value);
        }
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
        public static IEnumerable<tSource> UniqueBy<tSource, tKey>(this IEnumerable<tSource> src, Func<tSource, tKey> keySelecta)
        {
            HashSet<tKey> res = new HashSet<tKey>();
            foreach (tSource e in src)
            {
                tKey k = keySelecta(e);
                if (res.Contains(k))
                    continue;
                res.Add(k);
                yield return e;
            }
        }
        public static string EncryptID(this int id)
        {
            string guid = Guid.NewGuid().ToString().Replace("&", "0");
            string gID = guid.Remove(0, 4);
            string sID = id.ToString() + "&&_";
            //string eID = sID.Encrypt().Contains("-") ? sID.Encrypt().Replace("-", "_&").Replace("/", "_&&") : sID.Encrypt().Replace("/", "_&&"); // in case encryption generates '-'
            return string.Concat(sID, gID);
        }
        public static int DecryptID(this string gid)
        {
            if (string.IsNullOrEmpty(gid))
            {
                return -1;
            }
            int index = gid.IndexOf('&');
            string eID = gid.Substring(0, index);
            //eID = eID.Replace("_&", "-").Replace("_&&", "/");
            return int.Parse(eID);
        }
        public static string GetEnumDisplayName(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DisplayAttribute[] attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Name;
            else
                return value.ToString();
        }
        public static void Move<T>(this List<T> list, T item, int newIndex)
        {
            if (item != null)
            {
                var oldIndex = list.IndexOf(item);
                if (oldIndex > -1)
                {
                    list.RemoveAt(oldIndex);

                    if (newIndex > oldIndex) newIndex--;
                    // the actual index could have shifted due to the removal

                    list.Insert(newIndex, item);
                }
            }

        }
        public static string ToLowerandRemoveSpace(this string value)
        {
            return !string.IsNullOrEmpty(value) ? value.ToLower().Replace(" ", "") : value;
        }
        public static string CheckPasswordStrength(this string pass)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(pass))
            {
                //checking length
                if (pass.Length < 8)
                {
                    result = "Minimum length should be 8 characters in password";
                    return result;
                }
                //checking space in string
                if (pass.Contains(" "))
                {
                    result = "Space Not Allowed In Password";
                    return result;
                }
                //checking at least 1 small case character
                bool checkSmallCase = pass.Any(char.IsLower);
                if (!checkSmallCase)
                {
                    result = "At least 1 small case character required in password";
                    return result;
                }
                //checking at least 1 upper case character
                bool checkUpperCase = pass.Any(char.IsUpper);
                if (!checkUpperCase)
                {
                    result = "At least 1 upper case character required in password";
                    return result;
                }
                //checking at least 1 special character
                Regex rgxSpecial = new Regex("[^A-Za-z0-9]");
                bool hasSpecialChars = rgxSpecial.IsMatch(pass);
                if (!hasSpecialChars)
                {
                    result = "At least 1 special character required in password";
                    return result;
                }
                //checking at least 1 numeric character
                bool checkNumeric = pass.Any(c => char.IsDigit(c));
                if (!checkNumeric)
                {
                    result = "At least 1 numeric value in password";
                    return result;
                }
                result = "ok";
            }
            return result;
        }
    }
}
