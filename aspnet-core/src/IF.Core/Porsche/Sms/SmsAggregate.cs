using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    [Table("porsche_sms")]
    public class SmsAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 短信信息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 接口返回信息
        /// </summary>
        public string ResultMsg { get; set; }
        /// <summary>
        /// 接口返回代码
        /// </summary>
        public string ResultCode { get; set; }
        public int EffectiveTime { get; set; }
    }
}
