using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Mail;

namespace Utility
{
    public static class Helper
    {
        static List<Tuple<int, int>> listOfTuples = new List<Tuple<int, int>>();
        //char 0 to 9
        static Tuple<int, int> tupleDigits = Tuple.Create(48, 57);

        //char @, A to Z
        static Tuple<int, int> tupleCapitalChars = Tuple.Create(64, 90);
        //char a to z
        static Tuple<int, int> tupleSmallChars = Tuple.Create(97, 122);

        const string HTML_TAG_PATTERN = "<.*?>";

        static Helper()
        {
            listOfTuples.Add(tupleDigits);
            listOfTuples.Add(tupleCapitalChars);
            listOfTuples.Add(tupleSmallChars);
        }

        public static string ToFirstUpper(this String param_)
        {
            if (!String.IsNullOrEmpty(param_) && !String.IsNullOrWhiteSpace(param_))
            {
                return param_.Substring(0, 1).ToUpper() + param_.Substring(1);
            }
            return String.Empty;
        }

        public static string ToFirstUpperAndRestLower(this String param_)
        {
            if (!String.IsNullOrEmpty(param_) && !String.IsNullOrWhiteSpace(param_))
            {
                return param_.Substring(0, 1).ToUpper() + param_.Substring(1).ToLower();
            }
            return String.Empty;
        }

        public static string GetRandomAlphaNumericPassword()
        {
            Random rnd = new Random();
            char[] randomChars = new char[8];

            for (int i = 0; i < randomChars.Length; i++)
            {
                int tupleToPick = rnd.Next(0, listOfTuples.Count);
                randomChars[i] = (char)rnd.Next(listOfTuples[tupleToPick].Item1, listOfTuples[tupleToPick].Item2);
            }

            return new string(randomChars);
        }

        public static string GetRandomAlphaNumericSMSToken()
        {
            Random rnd = new Random();
            char[] randomChars = new char[4];

            for (int i = 0; i < randomChars.Length; i++)
            {
                int tupleToPick = rnd.Next(0, listOfTuples.Count);
                randomChars[i] = (char)rnd.Next(listOfTuples[tupleToPick].Item1, listOfTuples[tupleToPick].Item2);
            }

            return new string(randomChars);
        }

        public static List<Uri> FetchLinksFromSource(string htmlSource_)
        {
            List<Uri> links = new List<Uri>();
            string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
            MatchCollection matchesImgSrc = Regex.Matches(htmlSource_, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in matchesImgSrc)
            {
                string href = m.Groups[1].Value;
                links.Add(new Uri(href));
            }
            return links;
        }

        public static string StripHTML(string inputString_)
        {
            return Regex.Replace
              (inputString_, HTML_TAG_PATTERN, String.Empty);
        }

        public static string StripHTMLAndRemoveSymbols(string inputString_, params object[] symbolsToRemove_)
        {
            string result = Regex.Replace
              (inputString_, HTML_TAG_PATTERN, String.Empty);

            foreach (string symbol in symbolsToRemove_)
                result = result.Replace(symbol, " ");

            return result;
        }

        public static string GetMaskedCardNumber(string cardNumber_)
        {
            string result = String.Empty;

            if (!String.IsNullOrEmpty(cardNumber_) && cardNumber_.Length > 4)
            {
                result = "****" + cardNumber_.Substring(cardNumber_.Length - 4);
            }

            return result;
        }

        public static bool SplitAndMatchValue(this string delimitedString_, string valueToMatch_, char splitOn_)
        {
            if (!String.IsNullOrEmpty(delimitedString_))
            {
                string[] strArr = delimitedString_.Split(splitOn_);
                foreach (string str in strArr)
                {
                    if (str.ToLower() == valueToMatch_.ToLower())
                        return true;
                }
            }
            return false;
        }

        public static string ValIfNullOrEmpty(string input_, bool trim_, string valToReturn_)
        {
            if (String.IsNullOrEmpty(input_) || String.IsNullOrWhiteSpace(input_))
                return valToReturn_;

            if (trim_)
                return input_.Trim();
            else
                return input_;
        }

        public static int GetStartRowNumber(int pageNumber_) { return GetEndRowNumber(pageNumber_) - ApplicationSettings.PageSize + 1; }

        public static int GetEndRowNumber(int pageNumber_) { return pageNumber_ * ApplicationSettings.PageSize; }

        public static int GetNumberOfPages(int recordsCount_) { return (int)Math.Ceiling((double)recordsCount_ / (double)ApplicationSettings.PageSize); }

        public static string GetCombinedAddress(string street, string city, string state, int pincode, string country)
        {
            var result = street + "<br>" + city + "<br>" + state + "<br>" + pincode + "<br>" + country;
            return result;
        }

        public static string[] GetArraySplitByCharacter(string str, char keyword)
        {
            string[] word = str.Split(keyword);
            return word;
        }

        public static string GetDirectoryName(int type, int schoolID, int teacherID)
        {
            switch (type)
            {
                case 1:
                    return "School-" + schoolID + "/Teacher-" + teacherID + "/Audio/";
                case 2:
                    return "School-" + schoolID + "/Teacher-" + teacherID + "/Animation/";
                case 3:
                    return "School-" + schoolID + "/Teacher-" + teacherID + "/Activity/";
                case 4:
                    return "School-" + schoolID + "/Teacher-" + teacherID + "/Text/";
                case 5:
                    return "School-" + schoolID + "/Teacher-" + teacherID + "/Video/";
                default:
                    return "School-" + schoolID + "/Teacher-" + teacherID + "/misc/";
            }
        }

        public static string EncryptString(string Message, string Passphrase)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToEncrypt = UTF8.GetBytes(Message);
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }
            return Convert.ToBase64String(Results);
        }

        public static string DecryptString(string Message, string Passphrase)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToDecrypt = Convert.FromBase64String(Message);
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }
            return UTF8.GetString(Results);
        }

        public static string CategoryImagePath()
        {
            var path = "~/CDN/Hangout/CategoryImages/";
            return path;
        }

        public static string AdvertisementImagePath()
        {
            var path = "~/CDN/Hangout/AdvertisementImages/";
            return path;
        }

        public static string UserAdvertisementImagePath(int userID)
        {
            return "~/CDN/UI/AdvertisementImages/User-" + userID + "/";
        }

        public static string DebateAdvertisementImagePath(int userID)
        {
            var path = "~/CDN/Hangout/DebateAdvertisementImages/User-" + userID + "/";
            return path;
        }

        public static string ConvertDateTimeToDate(DateTime Date_)
        {
            string date = Date_.ToString("dddd,  dd MMMM yyyy", CultureInfo.InvariantCulture);
            return date;
        }

        public static bool SendVerificationCodeToMail(string u_mail, string v_code)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                message.From = new MailAddress("info@hang-out.in");
                message.To.Add(new MailAddress(u_mail));
                message.Subject = "Hang-Out Verification Link";
                message.Body = "<a href=\"hang-out.in/account/verifyuser?str=" + v_code + "\">Click here to verify your E-mail ID</a>";
                SmtpClient smtp = new SmtpClient("hang-out.in");
                smtp.Credentials = new System.Net.NetworkCredential("info@hang-out.in", "nFt!0f78");
                smtp.Port = 25;
                smtp.EnableSsl = false;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static string officeMemberName(int id)
        {
            switch (id)
            {
                case 1:
                    return "Visual By";
                case 2:
                    return "Tested By";
                case 3:
                    return "Process engg.";
                case 4:
                    return "Display Prog No.";
                default:
                    return "";
            }
        }

        public static string TestingStage(int id)
        {
            switch (id)
            {
                case 1:
                    return "Testing";
                case 2:
                    return "Qc";
                case 3:
                    return "Oqc";
                default:
                    return "";
            }
        }

        public static string ProductionLine(int id)
        {
            switch (id)
            {
                case 1:
                    return "Card";
                case 2:
                    return "Assembly";
                default:
                    return "";
            }
        }
    }
}
