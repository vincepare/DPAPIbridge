/**
 * DPAPIBridge
 * Use Windows Data Protection API from command line
 * Requires .NET Framework 2.0
 * @author Vincent Paré
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using NDesk.Options;
using System.IO;

namespace DPAPIbridge
{
    class Program
    {
        static void Main(string[] args)
        {
            // Arguments initialization
            bool base64 = false;
            string mode = null;
            string input = null;
            bool show_help = false;
            DPAPI.KeyType key_type = DPAPI.KeyType.UserKey;
            byte[] inputBytes = new byte[0];
            string outputFile = null;

            // Reading arguments
            var p = new OptionSet() {
                {"e|encrypt", "Encrypt input data", delegate (string v) {mode = "encrypt"; }},
                {"d|decrypt", "Decrypt input data", delegate (string v) {mode = "decrypt"; }},
                {"i|input=", "Get input data from this argument (rather than stdin)", delegate (string v) {if (v != null) input = v; }},
                {"b|base64", "Encrypt mode : handle input as base64 encoded data. Decrypt mode : output base64-encoded result. Use it to avoid troubles when clear data contains non ASCII bytes, like binary data.", delegate (string v) {if (v != null) base64 = true; }},
                {"o|output=", "Send output to file (instead of stdout)", delegate (string v) {if (v != null) outputFile = v; }},
                {"m|machine", "Use DPAPI machine key (instead of user key)", delegate (string v) {if (v != null) key_type = DPAPI.KeyType.MachineKey; }},
                { "?|h|help",  "Show this message and exit", v => show_help = v != null },
            };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Error.WriteLine(e.Message);
                return;
            }

            // Print help message
            if (show_help)
            {
                Console.WriteLine("Usage: dpapibridge (--encrypt|--decrypt) [--base64] [--input=]");
                Console.WriteLine("Options:");
                p.WriteOptionDescriptions(Console.Out);
                return;
            }

            // Get input bytes
            if (input == null)
            {
                // Checking standard input for data
                bool inputAvailable = false;
                try
                {
                    inputAvailable = Console.KeyAvailable;
                }
                catch (InvalidOperationException)
                {
                    inputAvailable = true;
                }
                if (!inputAvailable)
                {
                    Console.Error.WriteLine("No data available on standard input");
                    return;
                }
                
                // Reading standard input
                using (Stream stdin = Console.OpenStandardInput())
                {
                    byte[] buffer = new byte[2048];
                    int readBytes;
                    while ((readBytes = stdin.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // Resize input array + content transfer
                        byte[] newInputBytes = new byte[inputBytes.Length + readBytes];
                        Buffer.BlockCopy(inputBytes, 0, newInputBytes, 0, inputBytes.Length);

                        // Append new data (buffer)
                        Buffer.BlockCopy(buffer, 0, newInputBytes, inputBytes.Length, readBytes);
                        inputBytes = newInputBytes;
                    }
                }
            }
            else
            {
                // Convert --input from string to byte[]
                inputBytes = Encoding.UTF8.GetBytes(input);
            }

            // Dispatch
            switch (mode)
            {
                case "encrypt":
                    Encrypt(inputBytes, base64, key_type);
                    break;
                case "decrypt":
                    Decrypt(inputBytes, base64, outputFile);
                    break;
                default:
                    Console.Error.WriteLine("Mode not set (you must either choose --encrypt or --decrypt)");
                    break;
            }
        }

        /**
         * Encrypts clearBytes and prints encrypted data (base64 encoded)
         * @param byte[] clearBytes : Clear data to encrypt
         * @param bool base64 : Tells if clearBytes contains base64-encoded data (true) or raw data (false)
         */
        static void Encrypt(byte[] clearBytes, bool base64, DPAPI.KeyType key_type)
        {
            // base64 decode clearBytes
            if (base64)
            {
                string base64string = Encoding.UTF8.GetString(clearBytes);
                try
                {
                    clearBytes = Convert.FromBase64String(base64string);
                }
                catch (FormatException)
                {
                    Console.Error.WriteLine("Input data cannot be read as base64");
                    return;
                }
            }

            // Encryption
            byte[] entropy = null;
            byte[] encrypted = DPAPI.Encrypt(key_type, clearBytes, entropy, "Encrypted with Windows DPAPI through dpapibridge");

            // Print result
            string encryptedBase64 = Convert.ToBase64String(encrypted);
            Console.Out.WriteLine(encryptedBase64);
        }

        /**
         * Decrypts encryptedBytes and prints clear data
         * @param byte[] encryptedBytes : Encrypted data to decrypt, base64-encoded
         * @param bool base64 : Encode output as base64 if true (useful when clear data contains non ASCII bytes)
         * @param string outputFile : File path to send output
         */
        static void Decrypt(byte[] encryptedBytes, bool base64, string outputFile)
        {
            // base64 decode encryptedBytes
            string base64string = Encoding.UTF8.GetString(encryptedBytes);
            try
            {
                encryptedBytes = Convert.FromBase64String(base64string);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("Cannot base64-decode input");
                return;
            }
            

            // Decryption
            byte[] entropy = null;
            string description;
            byte[] decrypted;
            try
            {
                decrypted = DPAPI.Decrypt(encryptedBytes, entropy, out description);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return;
            }

            // Save output to file
            string output;
            if (outputFile != null)
            {
                if (base64)
                {
                    output = Convert.ToBase64String(decrypted);
                    decrypted = Encoding.UTF8.GetBytes(output);
                }
                File.WriteAllBytes(outputFile, decrypted);
                Console.WriteLine("output saved to" + outputFile);
                return;
            }

            // Print result
            if (base64)
            {
                output = Convert.ToBase64String(decrypted);
            }
            else
            {
                output = Encoding.UTF8.GetString(decrypted);
            }
            Console.Out.WriteLine(output);
        }
    }
}
