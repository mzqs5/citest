using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    /// <summary>
    /// 活动
    /// </summary>
    [Table("porsche_activity")]
    public class ActivityAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 0 活动 1票务 2最新消息
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 0 有效 1过期
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 活动参数
        /// </summary>
        public string Option { get; set; }

        /// <summary>
        /// 活动参数英文
        /// </summary>
        public string EOption { get; set; }

        /// <summary>
        /// 活动标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 移动端封面图
        /// </summary>
        public string MobileImgUrl { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// 移动端缩略图
        /// </summary>
        public string MobileThumbnailUrl { get; set; }

        /// <summary>
        /// 移动端轮播图
        /// </summary>
        [StringLength(40000)]
        public string MobileImgs { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500)]
        public string Desc { get; set; }

        /// <summary>
        /// 轮播图
        /// </summary>
        [StringLength(40000)]
        public string Imgs { get; set; }

        /// <summary>
        /// 活动详情
        /// </summary>
        [StringLength(40000)]
        public string BackDetail { get; set; }

        /// <summary>
        /// 活动详情
        /// </summary>
        [StringLength(40000)]
        public string PerDetail { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        [StringLength(40000)]
        public string IntroDetail { get; set; }

        public string Link { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 地点
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 表单名称
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// 活动标题英文
        /// </summary>
        public string ETitle { get; set; }

        /// <summary>
        /// 描述英文
        /// </summary>
        [StringLength(500)]
        public string EDesc { get; set; }

        /// <summary>
        /// 日期英文
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// 地点英文
        /// </summary>
        public string EAddress { get; set; }

        /// <summary>
        /// 表单名称英文
        /// </summary>
        public string EFromName { get; set; }

        /// <summary>
        /// 活动详情英文
        /// </summary>
        [StringLength(40000)]
        public string EBackDetail { get; set; }

        /// <summary>
        /// 活动详情英文
        /// </summary>
        [StringLength(40000)]
        public string EPerDetail { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        [StringLength(40000)]
        public string EIntroDetail { get; set; }
    }

}
