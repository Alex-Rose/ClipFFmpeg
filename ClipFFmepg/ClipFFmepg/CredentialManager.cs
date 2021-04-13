using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ClipFFmpeg
{
    public class CredentialManager
    {
        private static CredentialManager instance;
        private const string SALT = "kTbK3V58^SaSCO4iSrHGyQ9&h@i&&*KMChzWD%92qx4HEiezLJWG6%Qwk6F7LUu!";

        public static CredentialManager Instance 
        { 
            get
            {
                if (instance == null)
                {
                    instance = new CredentialManager();
                }

                return instance;
            }
        }

        private CredentialManager()
        {
            string username = Properties.Settings.Default.Username;
            string password = Properties.Settings.Default.Password;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                try
                {

                    Username = Decrypt(username);
                    Password = Decrypt(password);
                    IsSet = true;
                } 
                catch
                {
                    Username = string.Empty;
                    Password = string.Empty;
                }
            }
        }

        public void PersistCredentials()
        {
            Properties.Settings.Default.Username = Encrypt(Username);
            Properties.Settings.Default.Password = Encrypt(Password);
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool IsSet { get; set; }

        private string Encrypt(string value)
        {
            try
            {
                //Create a file stream
                using (var memoryStream = new MemoryStream())
                {
                    using (StreamWriter myStream = new StreamWriter(memoryStream))
                    {
                        //Create a new instance of the default Aes implementation class  
                        // and configure encryption key.  
                        using (Aes aes = Aes.Create())
                        {
                            byte[] key = Encoding.UTF8.GetBytes(Properties.Resources.HashKey);
                            aes.Key = key;

                            //Stores IV at the beginning of the file.
                            //This information will be used for decryption.
                            byte[] iv = aes.IV;
                            memoryStream.Write(iv, 0, iv.Length);

                            //Create a CryptoStream, pass it the FileStream, and encrypt
                            //it with the Aes class.  
                            using (CryptoStream cryptStream = new CryptoStream(
                                memoryStream,
                                aes.CreateEncryptor(),
                                CryptoStreamMode.Write))
                            {

                                //Create a StreamWriter for easy writing to the
                                //file stream.  
                                using (StreamWriter sWriter = new StreamWriter(cryptStream))
                                {
                                    //Write to the stream.  
                                    sWriter.Write(value);
                                    sWriter.Write(SALT);
                                }

                                string encrypted = Convert.ToBase64String(memoryStream.ToArray());

                                //Inform the user that the message was written  
                                //to the stream.  
                                Console.WriteLine("The file was encrypted. " + encrypted);
                                return encrypted;
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                //Inform the user that an exception was raised.  
                Console.WriteLine("The encryption failed.");
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private string Decrypt(string encrypted)
        {
            try
            {
                //Create a file stream.
                using (MemoryStream myStream = new MemoryStream(Convert.FromBase64String(encrypted)))
                {
                    //Create a new instance of the default Aes implementation class
                    using (Aes aes = Aes.Create())
                    {

                        //Reads IV value from beginning of the file.
                        byte[] iv = new byte[aes.IV.Length];
                        myStream.Read(iv, 0, iv.Length);

                        //Create a CryptoStream, pass it the file stream, and decrypt
                        //it with the Aes class using the key and IV.
                        byte[] key = Encoding.UTF8.GetBytes(Properties.Resources.HashKey);
                        using (CryptoStream cryptStream = new CryptoStream(
                           myStream,
                           aes.CreateDecryptor(key, iv),
                           CryptoStreamMode.Read))
                        {

                            //Read the stream.
                            using (StreamReader sReader = new StreamReader(cryptStream))
                            {
                                string decrypted = sReader.ReadToEnd();
                                if (!decrypted.Contains(SALT))
                                {
                                    throw new Exception("Invalid payload");
                                }

                                decrypted = decrypted.Substring(0, decrypted.Length - SALT.Length);
                                //Display the message.
                                Console.WriteLine("The decrypted original message: {0}", decrypted);
                                return decrypted;
                            }
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("The decryption failed.");
                throw;
            }
        }
    }
}
