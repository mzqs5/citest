
using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
namespace IF.Porsche
{
    [Table("IFFile")]
    public class FileAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 文件地址
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件二进制数据
        /// </summary>
        public byte[] FileBuffer { get; set; }

        /// <summary>
        /// 上传主机地址
        /// </summary>
        public string HostAddress { get; set; }
    }
}
