using System;
using System.IO;
using System.Security.Cryptography;

namespace ReadDatFile
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define the path of the .dat file
            string datFilePath = @"D:\2200_e_mcb21.dat";

            // Define the key of the .dat file as a hexadecimal string
            string keyDatFileHex = "8FFC5B32E3896D0C05A9C479BEDC95F5";

            // Convert the key of the .dat file from hexadecimal to byte array
            byte[] keyDatFile = HexStringToByteArray(keyDatFileHex);

            // Create a file stream to read the .dat file
            using (FileStream datFileStream = File.OpenRead(datFilePath))

            {
                // Get the length of the .dat file in bytes
                int datFileLength = (int)datFileStream.Length;

                // Create a byte array to store the original .dat file content
                byte[] origDatFileContent = new byte[datFileLength];

                // Read the .dat file content into the byte array
                datFileStream.Read(origDatFileContent, 0, datFileLength);

                // Set the first 16 bytes of origDatFileContent to the IV
                byte[] ivDatFile = new byte[16];
                Array.Copy(origDatFileContent, 0, ivDatFile, 0, 16);

                // Set the rest part (from 17th bytes to end) to DatFile2Decrypt
                byte[] datFile2Decrypt = new byte[datFileLength - 16];
                Array.Copy(origDatFileContent, 16, datFile2Decrypt, 0, datFileLength - 16);

                // Create an instance of the Aes class with AES/CBC/No Padding algorithm
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.None;

                    // Create an instance of the ICryptoTransform class with the key and IV
                    using (ICryptoTransform decryptor = aes.CreateDecryptor(keyDatFile, ivDatFile))
                    {
                        // Create a memory stream to store the decrypted data
                        using (MemoryStream msDecrypt = new MemoryStream())
                        {
                            // Create a crypto stream to perform the decryption
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                            {
                                // Write the data to decrypt to the crypto stream
                                csDecrypt.Write(datFile2Decrypt, 0, datFile2Decrypt.Length);
                                csDecrypt.FlushFinalBlock();

                                // Get the decrypted data from the memory stream as a byte array
                                byte[] decryptedData = msDecrypt.ToArray();

                                // Get the first 32 bytes of the decrypted data as the key of encrypted PIN
                                byte[] keyEncryptedPin = new byte[32];
                                Array.Copy(decryptedData, 0, keyEncryptedPin, 0, 32);

                                // Convert the key of encrypted PIN from byte array to hexadecimal string
                                string keyEncryptedPinHex = ByteArrayToHexString(keyEncryptedPin);

                                // Print the key of encrypted PIN as a hexadecimal string
                                Console.WriteLine("Key of encrypted PIN: " + keyEncryptedPinHex);
                            }
                        }
                    }
                }
            }
        }

        // A helper method to convert a hexadecimal string to a byte array
        public static byte[] HexStringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        // A helper method to convert a byte array to a hexadecimal string
        public static string ByteArrayToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}
