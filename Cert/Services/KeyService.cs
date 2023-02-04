using System;
using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Cert.Services
{
    public class KeyService
    {
        public byte[] Sign(byte[] data, byte[] key)
        {

            // Load the private key
            AsymmetricKeyParameter privateKey = ...;

            // Create a signature engine
            ISigner signer = SignerUtilities.GetSigner("DSTU4145");
            signer.Init(true, privateKey);

            // Sign the data
            byte[] data = ...;
            signer.BlockUpdate(data, 0, data.Length);
            byte[] signature = signer.GenerateSignature();

            Console.WriteLine("Data signed successfully.");
            Console.ReadLine();
        }
    }
}

}
}
