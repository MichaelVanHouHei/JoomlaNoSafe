using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using HttpCodeLib;

namespace JoomlaNoSafe
{
    public class joomlaFucker
    {
        private CookieContainer cc { get; set; }
        public  List<jItem> exps = new List<jItem>();
        private string path => "{0}/components/{1}";
        private string exploitDB =>
            "https://raw.githubusercontent.com/offensive-security/exploit-database/master/files.csv";

        private string key => "nosafebymichaelvan";
        private string joomlaExps =>
            "https://raw.githubusercontent.com/rapid7/metasploit-framework/master/data/wordlists/joomla.txt";

        public joomlaFucker()
        {
            if (!File.Exists("info.nosafe")) getinfo();
            if (!File.Exists("exps.nosafe")) getExps();
        }

        public void updateExps()
        {
            getinfo();
            getExps();
        }
        public void getinfo()
        {
            System.IO.File.WriteAllText(@"info.nosafe", StringCipher.Encrypt(new XJHTTP().GetHtml(exploitDB).Html, key));
        }

        public void getExps()
        {
            System.IO.File.WriteAllText(@"exps.nosafe", StringCipher.Encrypt(new XJHTTP().GetHtml(joomlaExps).Html, key));
        }

        public  string dumpString(string input ,int index , out string info)
        {
            var sb = new StringBuilder();
            var split = input.Split('-');
            for(int i = index ; i <= split[0].Length-1 ; i++)
            {
                
                if (input[i].ToString() == @"-" || input[i].ToString() ==@"""" || input[i].ToString() ==@" " ||input[i].ToString() ==@"'" || input[i].ToString() ==",")
                {
                    break;
                }
                sb.Append(input[i]);
            }
            info = split[1];
            return sb.ToString();
        }

        public  void  Scan(string link,string exp ,string shit,Action<bool> action)
        {
            //(int count, string host, int port, string request, int timeout, string encoding, bool foward_302
            //string.Format(path,link,exp) 
          
            //var s = new XJHTTP().GetHtml(string.Format(path, link, exp));
          //  ThreadPool.QueueUserWorkItem(_ =>
         //   {
                new XJHTTP().AsyncGetHtml(new HttpItems()
                {
                    URL = string.Format(path, link, exp),
                    UserAgent="GoogleBot",


                }, s =>
                {
                    if (s.StatusCode != HttpStatusCode.NotFound && !check404(s.Html) &&
                        s.Html != shit && !string.IsNullOrEmpty(s.Html) &&
                        s.Html.Length < 5000 && s.StatusCode == HttpStatusCode.Found)
                    {

                        action(true);
                    }
                    else
                    {
                        action(false);
                    }
                });
        //    });

        }
        public void loadExps()
        {
            exps.Clear();
             var expLines = StringCipher.Decrypt(File.ReadAllText("info.nosafe"), key)
                .Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None).Where(x => x.Contains("Joomla") && x.Contains("com_") ).Select(y => y.Split(new[] { "," }, StringSplitOptions.None));
          
            foreach (var res in expLines)
            {
                var expINfo = res[2].ToString();
                var info = "";
                exps.Add(new jItem()
                {
                    comName = dumpString(expINfo , expINfo.IndexOf(@"com_") ,out info),
                    info =   info, 
                    link = string.Format("https://www.exploit-db.com/exploits/{0}/",res[0]),
                });
            }
        }

        public bool check404(string input)
        {
            if (input.Contains("<h1>Not Found</h1>") || input.Contains("<title> 404 - Page not found</title>") ||input.Contains("\"center error-404\""))
            {
                return true;
            }
            return false;
        }
		
        public string getToekn(string response)
        {
            return new Regex(@"name=""([a-f0-9]{32})").Match(response).Groups[1].Value;
        }
    }
    public static class StringCipher
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
    public class jItem
    {
        public string comName { get; set; }
        public string link { get; set; }
        public string info { get; set; }
      
    }
}
