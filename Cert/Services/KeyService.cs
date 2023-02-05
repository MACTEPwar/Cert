using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Cert.Services
{
    public class KeyService
    {
        public void Read()
        {
            string path = @"C:/Users/macte/Downloads/3111019395_3111019395_P230125100258.ZS2";
            string password = "3111019395";

            string privateKeyFilePath = path;

            // Read the private key from the PEM file
            AsymmetricCipherKeyPair keyPair;
            using (var reader = File.OpenText(privateKeyFilePath))
            {
                var pemReader = new PemReader(reader, new PasswordFinder(password));
                keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            }

            // Extract the private key from the key pair
            RsaPrivateCrtKeyParameters privateKey = (RsaPrivateCrtKeyParameters)keyPair.Private;

            Console.WriteLine("Private Key:");
            Console.WriteLine("Modulus: " + privateKey.Modulus);
            Console.WriteLine("Exponent: " + privateKey.Exponent);
        }

        class PasswordFinder : IPasswordFinder
        {
            private readonly string password;

            public PasswordFinder(string password)
            {
                this.password = password;
            }

            public char[] GetPassword()
            {
                return password.ToCharArray();
            }
        }

        //public void Read()
        //{
        //    string path = @"C:/Users/macte/Downloads/3111019395_3111019395_P230125100258.ZS2";
        //    byte[] encryptedData;
        //    byte[] iv;

        //    int offset;
        //    int dataLength;
        //    int ivOffset;
        //    int ivLength;

        //    using (FileStream fileStream = new FileStream(path, FileMode.Open))
        //    {
        //        using (BinaryReader binaryReader = new BinaryReader(fileStream))
        //        {
        //            // Read the header and extract the values for offset, dataLength, ivOffset, and ivLength
        //            offset = binaryReader.ReadInt32();
        //            dataLength = binaryReader.ReadInt32();
        //            ivOffset = binaryReader.ReadInt32();
        //            ivLength = binaryReader.ReadInt32();
        //        }
        //    }

        //    using (FileStream fileStream = new FileStream(path, FileMode.Open))
        //    {
        //        using (BinaryReader binaryReader = new BinaryReader(fileStream))
        //        {
        //            // Seek to the location of the encrypted data in the file
        //            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

        //            // Read the encrypted data
        //            encryptedData = binaryReader.ReadBytes(dataLength);

        //            // Seek to the location of the initialization vector in the file
        //            binaryReader.BaseStream.Seek(ivOffset, SeekOrigin.Begin);

        //            // Read the initialization vector
        //            iv = binaryReader.ReadBytes(ivLength);
        //        }
        //    }

        //    byte[] password = Encoding.UTF8.GetBytes("3111019395");
        //    byte[] decryptedData = Decrypt(encryptedData, password, iv);

        //}

        //public byte[] Decrypt(byte[] encryptedData, byte[] password, byte[] iv)
        //{
        //    IBufferedCipher cipher = new BufferedBlockCipher(new CbcBlockCipher(new AesEngine()));
        //    cipher.Init(false, new ParametersWithIV(new KeyParameter(password), iv));
        //    return cipher.DoFinal(encryptedData);
        //}

        //public byte[] ReadIv()
        //{
        //    return new byte[] { 0x05, 0x06, 0x07, 0x08 };
        //}

        //public byte[] Sign(byte[] data, byte[] key)
        //{

        //    // Load the private key
        //    AsymmetricKeyParameter privateKey = null;

        //    // Create a signature engine
        //    ISigner signer = SignerUtilities.GetSigner("DSTU4145");
        //    signer.Init(true, privateKey);

        //    // Sign the data
        //    signer.BlockUpdate(data, 0, data.Length);
        //    byte[] signature = signer.GenerateSignature();

        //    return signature;

        //    Console.WriteLine("Data signed successfully.");
        //    Console.ReadLine();
        //}
    }
}
