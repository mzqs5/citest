using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IF.Payment.Notify
{
    [System.Xml.Serialization.XmlType(TypeName = "xml")]
    public class WXPayModelBasic
    {
        /// <summary>
        /// <para>说明：返回状态码</para>
        /// <para>示例值：SUCCESS/FAIL</para>
        /// <para>描述：此字段是通信标识，非交易标识，交易是否成功需要查看result_code来判断</para>
        /// </summary>
        public string return_code { get; set; }

        /// <summary>
        /// <para>说明：返回信息</para>
        /// <para>示例值：签名失败</para>
        /// <para>描述：返回信息，如非空，为错误原因等等...</para>
        /// </summary>
        public string return_msg { get; set; }
    }
}
