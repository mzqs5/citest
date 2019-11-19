using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Configuration;
using Abp.Zero.Configuration;
using DevExtreme.AspNet.Mvc;
using IF.Authorization.Users;
using IF.Porsche;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Mvc;
using Abp.Application.Services.Dto;
using Abp;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using IF.Configuration;
using IF.Web;
using System.Text;
using System.Net;
using System.IO;
using Abp.Authorization;
using IF.Authorization;

namespace IF.Porsche
{
    public class SmsAppService : IFAppServiceBase, ISmsAppService
    {
        IRepository<SmsAggregate> SmsRepository;
        public SmsAppService(
            IRepository<SmsAggregate> SmsRepository)
        {
            this.SmsRepository = SmsRepository;
        }

        /// <summary>
        /// 获取短信的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        [AbpAuthorize("Pages.Sms")]
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from Sms in SmsRepository.GetAll().AsEnumerable()
                         select new SmsDto
                         {
                             Mobile = Sms.Mobile,
                             Code = Sms.Code,
                             Msg = Sms.Msg,
                             ResultMsg = Sms.ResultMsg,
                             ResultCode = Sms.ResultCode,
                             CreationTime = Sms.CreationTime
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }


        #region 根据ID获取短信信息

        /// <summary>
        /// 根据ID获取短信信息
        /// </summary>
        /// <param name="id">短信信息主键</param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_Users)]
        public async Task<SmsDto> GetAsync(int id)
        {
            try
            {
                var Sms = await SmsRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (Sms == null)
                {
                    throw new EntityNotFoundException(typeof(SmsAggregate), id);
                }
                var dto = this.ObjectMapper.Map<SmsDto>(Sms);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("短信获取异常！", e);
                throw new AbpException("短信获取异常", e);
            }
        }
        #endregion

        #region 验证短信验证码
        /// <summary>
        /// 验证短信验证码（无需鉴权）
        /// </summary>
        /// <param name="Mobile">短信信息主键</param>
        /// <returns></returns>
        public async Task VerificationAsync(VerificationSmsDto dto)
        {
            try
            {
                var Sms = await SmsRepository.GetAll().Where(k => k.Mobile == dto.Mobile
                    && k.Code == dto.Code
                    && k.CreationTime.AddMinutes((Double)k.EffectiveTime) >= DateTime.Now)
                    .FirstOrDefaultAsync();

                if (Sms == null)
                {
                    Logger.Error("短信验证失败！");
                    throw new AbpException("短信验证失败!");
                }
            }
            catch (Exception e)
            {
                Logger.Error("短信获取异常！", e);
                throw new AbpException("短信获取异常", e);
            }
        }
        #endregion

        #region 批量删除短信
        
        /// <summary>
        /// 批量删除短信
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize(PermissionNames.Pages_Users)]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await SmsRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("短信删除异常！", e);
                throw new AbpException("短信删除异常！", e);
            }
        }
        #endregion

        #region 发送短信验证码
        /// <summary>
        /// 发送短信验证码（无需鉴权）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> SendAsync(SmsEditDto input)
        {
            try
            {
                var configuration = AppConfigurations.Get(Directory.GetCurrentDirectory());
                SmsAggregate entity = this.ObjectMapper.Map<SmsAggregate>(input);
                //if (input.EffectiveTime.HasValue)
                //    entity.EffectiveTime = input.EffectiveTime.Value;
                //else
                    entity.EffectiveTime = 5;
                var SmsConfig = configuration.GetSection("Sms");
                if (SmsConfig == null)
                    throw new AbpException("短信配置为空！");
                Random rad = new Random();
                int val_code = rad.Next(1000, 10000);
                entity.Code = val_code.ToString();
                string content = "您的验证码是：" + val_code + " 。请不要把验证码泄露给其他人。";
                entity.Msg = content;
                string postStrTpl = "account={0}&password={1}&mobile={2}&content={3}";

                UTF8Encoding encoding = new UTF8Encoding();
                byte[] postData = encoding.GetBytes(string.Format(postStrTpl, SmsConfig.GetSection("Account").Value, SmsConfig.GetSection("PassWord").Value, input.Mobile, content));

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(SmsConfig.GetSection("PostUrl").Value);
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.ContentLength = postData.Length;

                Stream newStream = myRequest.GetRequestStream();
                // Send the data.
                newStream.Write(postData, 0, postData.Length);
                newStream.Flush();
                newStream.Close();

                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();

                if (myResponse.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);

                    string res = reader.ReadToEnd();
                    int len1 = res.IndexOf("</code>");
                    int len2 = res.IndexOf("<code>");
                    string code = res.Substring((len2 + 6), (len1 - len2 - 6));
                    //Response.Write(code);

                    int len3 = res.IndexOf("</msg>");
                    int len4 = res.IndexOf("<msg>");
                    string msg = res.Substring((len4 + 5), (len3 - len4 - 5));
                    //Response.Write(msg);
                    entity.ResultCode = code;
                    entity.ResultMsg = msg;
                }
                else
                {
                    Logger.Error(string.Format("短信发送失败！{0}", myResponse.GetResponseStream()));
                    throw new AbpException(string.Format("短信发送失败！{0}", myResponse.GetResponseStream()));
                }
                await SmsRepository.InsertAsync(entity);
                await CurrentUnitOfWork.SaveChangesAsync();
                return new { ResultCode = entity.ResultCode, ResultMsg = entity.ResultMsg };
            }
            catch (Exception e)
            {
                Logger.Error("短信发送异常！", e);
                throw new AbpException("短信发送异常！", e);
            }
        }
        #endregion
    }

    public class VerificationSmsDto
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        public string Code { get; set; }
    }

    public class SmsEditDto
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 有效时长 默认5分钟
        /// </summary>
        //public int? EffectiveTime { get; set; }
    }

    public class SmsDto
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
        /// 短信接口返回信息
        /// </summary>
        public string ResultMsg { get; set; }
        /// <summary>
        /// 短信接口返回代码
        /// </summary>
        public string ResultCode { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime CreationTime { get; internal set; }
    }
}
