﻿using Cinchoo.PGP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoPGP.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //GenerateKeyPair();
            //EncryptFile();
            //DecryptFile();
            //Console.ReadLine();

            EncryptFileNSign();
            DecryptFileNVerify();
        }

        private static void GenerateKeyPair()
        {
            using (ChoPGPEncryptDecrypt pgp = new ChoPGPEncryptDecrypt())
                pgp.GenerateKey("pub.asc", "pri.asc", "mark@gmail.com", "Test123");

            Console.WriteLine("PGP KeyPair generated.");
        }

        private static void EncryptFile()
        {
            using (ChoPGPEncryptDecrypt pgp = new ChoPGPEncryptDecrypt())
            {
                pgp.EncryptFile("SampleData.txt", "SampleData.PGP", "Pub.asc", true, false);
                Console.WriteLine("PGP Encryption done.");
            }
        }
        private static void DecryptFile()
        {
            using (ChoPGPEncryptDecrypt pgp = new ChoPGPEncryptDecrypt())
            {
                //pgp.CompressionAlgorithm = ChoCompressionAlgorithm.Zip;
                pgp.DecryptFile("SampleData.PGP", "SampleData.OUT", "Pri.asc", "Test123");
                Console.WriteLine("PGP Decryption done.");
            }
        }
        private static void EncryptFileNSign()
        {
            using (ChoPGPEncryptDecrypt pgp = new ChoPGPEncryptDecrypt())
            {
                //pgp.CompressionAlgorithm = ChoCompressionAlgorithm.Zip;
                pgp.EncryptFileAndSign("SampleData.txt", "SampleData.PGP", "Pub.asc", "Pri.asc", "Test123", true, false);
                Console.WriteLine("PGP Encryption done.");
            }
        }


        private static void DecryptFileNVerify()
        {
            using (ChoPGPEncryptDecrypt pgp = new ChoPGPEncryptDecrypt())
            {
                pgp.DecryptFileAndVerify("SampleData.PGP", "SampleData.OUT", "Pub.asc", "Pri.asc", "Test123");
                Console.WriteLine("PGP Decryption done.");
            }
        }
    }
}
