﻿using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinchoo.PGP
{
    internal sealed class ChoPGPEncryptionKeys
    {
        #region Instance Members (Public)

        public PgpPublicKey PublicKey { get; private set; }
        public PgpPrivateKey PrivateKey { get; private set; }
        public PgpSecretKey SecretKey { get; private set; }

        #endregion Instance Members (Public)

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the EncryptionKeys class.
        /// Two keys are required to encrypt and sign data. Your private key and the recipients public key.
        /// The data is encrypted with the recipients public key and signed with your private key.
        /// </summary>
        /// <param name="publicKeyFilePath">The key used to encrypt the data</param>
        /// <param name="privateKeyFilePath">The key used to sign the data.</param>
        /// <param name="passPhrase">The password required to access the private key</param>
        /// <exception cref="ArgumentException">Public key not found. Private key not found. Missing password</exception>
        public ChoPGPEncryptionKeys(string publicKeyFilePath, string privateKeyFilePath, string passPhrase)
        {
            if (String.IsNullOrEmpty(publicKeyFilePath))
                throw new ArgumentException(nameof(publicKeyFilePath));
            if (String.IsNullOrEmpty(privateKeyFilePath))
                throw new ArgumentException(nameof(privateKeyFilePath));
            if (passPhrase == null)
                throw new ArgumentNullException("Invalid Pass Phrase.");

            if (!File.Exists(publicKeyFilePath))
                throw new FileNotFoundException(String.Format("Public Key file [{0}] does not exist.", publicKeyFilePath));
            if (!File.Exists(privateKeyFilePath))
                throw new FileNotFoundException(String.Format("Private Key file [{0}] does not exist.", privateKeyFilePath));

            PublicKey = ReadPublicKey(publicKeyFilePath);
            SecretKey = ReadSecretKey(privateKeyFilePath);
            PrivateKey = ReadPrivateKey(passPhrase);
        }

		public ChoPGPEncryptionKeys(Stream publicKeyFileStream, Stream privateKeyFileStream, string passPhrase)
		{
			if (publicKeyFileStream == null)
				throw new ArgumentException(nameof(publicKeyFileStream));
			if (privateKeyFileStream == null)
				throw new ArgumentException(nameof(privateKeyFileStream));
			if (passPhrase == null)
				throw new ArgumentNullException("Invalid Pass Phrase.");

			PublicKey = ReadPublicKey(publicKeyFileStream);
			SecretKey = ReadSecretKey(privateKeyFileStream);
			PrivateKey = ReadPrivateKey(passPhrase);
		}

		#endregion Constructors

		#region Secret Key

		private PgpSecretKey ReadSecretKey(Stream privateKeyStream)
		{
			using (Stream inputStream = PgpUtilities.GetDecoderStream(privateKeyStream))
			{
				PgpSecretKeyRingBundle secretKeyRingBundle = new PgpSecretKeyRingBundle(inputStream);
				PgpSecretKey foundKey = GetFirstSecretKey(secretKeyRingBundle);
				if (foundKey != null)
					return foundKey;
			}
			throw new ArgumentException("Can't find signing key in key ring.");
		}

		private PgpSecretKey ReadSecretKey(string privateKeyPath)
        {
            using (Stream sr = File.OpenRead(privateKeyPath))
            {
				return ReadSecretKey(sr);
            }
            throw new ArgumentException("Can't find signing key in key ring.");
        }

        /// <summary>
        /// Return the first key we can use to encrypt.
        /// Note: A file can contain multiple keys (stored in "key rings")
        /// </summary>
        private PgpSecretKey GetFirstSecretKey(PgpSecretKeyRingBundle secretKeyRingBundle)
        {
            foreach (PgpSecretKeyRing kRing in secretKeyRingBundle.GetKeyRings())
            {
                PgpSecretKey key = kRing.GetSecretKeys()
                    .Cast<PgpSecretKey>()
                    .Where(k => k.IsSigningKey)
                    .FirstOrDefault();
                if (key != null)
                    return key;
            }
            return null;
        }

		#endregion Secret Key

		#region Public Key

		private PgpPublicKey ReadPublicKey(Stream publicKeyStream)
		{
			using (Stream inputStream = PgpUtilities.GetDecoderStream(publicKeyStream))
			{
				PgpPublicKeyRingBundle publicKeyRingBundle = new PgpPublicKeyRingBundle(inputStream);
				PgpPublicKey foundKey = GetFirstPublicKey(publicKeyRingBundle);
				if (foundKey != null)
					return foundKey;
			}
			throw new ArgumentException("No encryption key found in public key ring.");
		}

		private PgpPublicKey ReadPublicKey(string publicKeyPath)
        {
            using (Stream keyIn = File.OpenRead(publicKeyPath))
            {
				return ReadPublicKey(keyIn);
            }
            throw new ArgumentException("No encryption key found in public key ring.");
        }

        private PgpPublicKey GetFirstPublicKey(PgpPublicKeyRingBundle publicKeyRingBundle)
        {
            foreach (PgpPublicKeyRing kRing in publicKeyRingBundle.GetKeyRings())
            {
                PgpPublicKey key = kRing.GetPublicKeys()
                    .Cast<PgpPublicKey>()
                    .Where(k => k.IsEncryptionKey)
                    .FirstOrDefault();
                if (key != null)
                    return key;
            }
            return null;
        }

        #endregion Public Key

        #region Private Key

        private PgpPrivateKey ReadPrivateKey(string passPhrase)
        {
            PgpPrivateKey privateKey = SecretKey.ExtractPrivateKey(passPhrase.ToCharArray());
            if (privateKey != null)
                return privateKey;

            throw new ArgumentException("No private key found in secret key.");
        }

        #endregion Private Key
    }

}