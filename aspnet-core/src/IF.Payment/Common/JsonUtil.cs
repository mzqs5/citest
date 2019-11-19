using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace IF.Payment.Common
{
    /// <summary>
    ///  基于 Newtonsoft.Json 封装常用方法
    /// </summary>
    public sealed class JsonUtil
    {
        /// <summary>
        /// .NET对象转换成JSON字符串
        /// </summary>
        /// <param name="v">.NET对象</param>
        /// <returns></returns>
        public static string ToJson(object v)
        {
            //JsonSerializerSettings s = new JsonSerializerSettings
            //{
            //    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            //};
            var s = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            return JsonConvert.SerializeObject(v, s);
        }

        /// <summary>
        /// JSON字符串 转 成.NET对象
        /// </summary>
        /// <param name="v">JSON字符串</param>
        /// <returns></returns>
        public static object ToObject(string v)
        {
            return JsonConvert.DeserializeObject(v);
        }

        /// <summary>
        /// JSON字符串 转 成.NET泛型
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="v">JSON字符串</param>
        /// <returns></returns>
        public static T ToObject<T>(string v)
        {
            return JsonConvert.DeserializeObject<T>(v);
        }

        /// <summary>
        /// 将json数据反序列化为Dictionary
        /// </summary>
        /// <param name="jsonData">json数据</param>
        /// <returns></returns>
        public static Dictionary<string, object> JsonToDictionary(string jsonData)
        {
            //实例化JavaScriptSerializer类的新实例
            //JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                //将指定的 JSON 字符串转换为 Dictionary<string, object> 类型的对象
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
