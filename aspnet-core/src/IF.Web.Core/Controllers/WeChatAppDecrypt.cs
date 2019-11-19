using IF.Models.TokenAuth;
using System;
using System.IO;
using System.Security.Cryptography;

namespace IF.Authorization
{
    /// <summary>  
    /// 处理微信小程序用户数据的签名验证和解密  
    /// </summary>  
    public class WeChatAppDecrypt
    {

        /// <summary>  
        /// 根据微信小程序平台提供的解密算法解密数据  
        /// </summary>  
        /// <param name="encryptedData">加密数据</param>  
        /// <param name="iv">初始向量</param>  
        /// <param name="sessionKey">从服务端获取的SessionKey</param>  
        /// <returns></returns>  
        public static string Decrypt(DecodeInfo loginInfo)
        {
            RijndaelManaged rijalg = new RijndaelManaged();
            //-----------------      
            //设置 cipher 格式 AES-128-CBC      
            rijalg.KeySize = 128;
            rijalg.Padding = PaddingMode.PKCS7;
            rijalg.Mode = CipherMode.CBC;
            rijalg.Key = Convert.FromBase64String(loginInfo.session_key);
            rijalg.IV = Convert.FromBase64String(loginInfo.iv);


            byte[] encryptedData = Convert.FromBase64String(loginInfo.encryptedData);
            //解密      
            ICryptoTransform decryptor = rijalg.CreateDecryptor(rijalg.Key, rijalg.IV);

            string result;

            using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        result = srDecrypt.ReadToEnd();
                    }
                }
            }

            return result;
        }
    }
}
