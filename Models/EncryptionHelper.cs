using System.Security.Cryptography;
using System.Text;
using System;
using System.IO;
using System.Text;

namespace CliniqonProject.Models
{
    public static class EncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890abcdef1234567890abcdef");

        private static readonly byte[] IV = Encoding.UTF8.GetBytes("abcdef1234567890");

       
    }

}


