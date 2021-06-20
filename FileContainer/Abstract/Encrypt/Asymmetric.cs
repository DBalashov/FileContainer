using System.IO;
using System.Security.Cryptography;

namespace FileContainer.Encrypt
{
    // class Asymmetric : IEncryptorDecryptor
    // {
    //     readonly AsymmetricAlgorithm algo;
    //     
    //     internal Asymmetric(AsymmetricAlgorithm algo) => this.algo = algo;
    //
    //     public byte[] Encrypt(byte[] data)
    //     {
    //         using var stm = new MemoryStream();
    //         using var cs  = new CryptoStream(stm, algo., CryptoStreamMode.Write);
    //         
    //         var       dst =algo.Encrypt(src, RSAEncryptionPadding.Pkcs1);
    //
    //         var z1 = rsa.Decrypt(dst, RSAEncryptionPadding.Pkcs1);
    //     }
    //
    //     public byte[] Decrypt(byte[] data) => data;
    // }
}