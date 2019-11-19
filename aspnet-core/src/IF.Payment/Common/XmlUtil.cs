using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace IF.Payment.Common
{
    /// <summary>
    /// XML 工具类
    /// </summary>
    public class XmlUtil
    {
        /// <summary>
        /// 创建 xml CATA 不格式化数据
        /// </summary>
        /// <param name="s">需要组装的数据</param>
        /// <returns></returns>
        public static string XmlCATA(string s)
        {
            return "<![CDATA[" + s + "]]>";
        }

        /// <summary>
        /// 根据XML字符串转换成对象
        /// <para>方法由类对应XML文档key对应，可使用特性[XmlType(TypeName = "xml")]对对象重命名</para>
        /// <para>可使用 XmlElement(ElementName = "key") 生成对应XML key的值</para>
        /// </summary>
        /// <typeparam name="T">Type类型</typeparam>
        /// <param name="xml">xml字符串</param>
        /// <returns></returns>
        public static T XmlToObect<T>(string xml)
        {
            using (StringReader sr = new StringReader(xml))
            {
                var xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(sr);
            }
        }

        /// <summary>
        /// 单个类遍历，不支持嵌套泛型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string SinClassToXml<T>(T t)
        {
            var Sxml = new StringBuilder();
            foreach (var dr in typeof(T).GetProperties())
            {
                if (null != dr.GetValue(t, null))
                {
                    Sxml.AppendFormat("<{0}>{1}</{0}>", dr.Name, dr.GetValue(t, null));
                }
            }
            return Sxml.ToString();
        }
    }
}
