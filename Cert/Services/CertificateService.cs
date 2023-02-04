using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace Cert.Services
{
    public class CertificateService
    {
        public X509Certificate2 Read(byte[] certificateFile)
        {
            X509Certificate2 certificate = new X509Certificate2(certificateFile);
            return certificate;
        }
    }

    //public X509Certificate2 DecryptX501Key(string certificateFile, string encryptedX501KeyFile)
    //{
    //    // Load the X.509 certificate
    //    X509Certificate2 certificate = new X509Certificate2(certificateFile);

    //    // Get the private key from the certificate
    //    RSA privateKey = certificate.GetRSAPrivateKey();

    //    // Read the encrypted X.501 key from file
    //    string encryptedX501Key = File.ReadAllText(encryptedX501KeyFile);

    //    // Decrypt the X.501 key using the private key
    //    byte[] decryptedKey = privateKey.Decrypt(Convert.FromBase64String(encryptedX501Key), RSAEncryptionPadding.OaepSHA1);

    //    Console.WriteLine("X.501 key decrypted successfully.");
    //    Console.WriteLine("Decrypted key: {0}", Convert.ToBase64String(decryptedKey));
    //}
}
