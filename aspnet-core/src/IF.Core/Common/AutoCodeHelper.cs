using System;
using System.Collections.Generic;
using System.Text;

namespace IF.Common
{
    public class AutoCodeHelper
    {
        public static string Create(string per, string last)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(per.ToUpper());
            sb.Append(DateTime.Now.ToString("yyyyMMddHHmm"));
            sb.Append(last.ToUpper());
            sb.Append(NextRandom(10000, 4));
            return sb.ToString();
        }

        public static string Create(string per)
        {
            return Create(per, "");
        }

        private static int NextRandom(int numSeeds, int length)
        {
            // Create a byte array to hold the random value.  
            byte[] randomNumber = new byte[length];
            // Create a new instance of the RNGCryptoServiceProvider.  
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            // Fill the array with a random value.  
            rng.GetBytes(randomNumber);
            // Convert the byte to an uint value to make the modulus operation easier.  
            uint randomResult = 0x0;
            for (int i = 0; i < length; i++)
            {
                randomResult |= ((uint)randomNumber[i] << ((length - 1 - i) * 8));
            }
            return (int)(randomResult % numSeeds) + 1;
        }
    }
}
