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
        /// ��ȡ���ŵ������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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


        #region ����ID��ȡ������Ϣ

        /// <summary>
        /// ����ID��ȡ������Ϣ
        /// </summary>
        /// <param name="id">������Ϣ����</param>
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
                Logger.Error("���Ż�ȡ�쳣��", e);
                throw new AbpException("���Ż�ȡ�쳣", e);
            }
        }
        #endregion

        #region ��֤������֤��
        /// <summary>
        /// ��֤������֤�루�����Ȩ��
        /// </summary>
        /// <param name="Mobile">������Ϣ����</param>
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
                    Logger.Error("������֤ʧ�ܣ�");
                    throw new AbpException("������֤ʧ��!");
                }
            }
            catch (Exception e)
            {
                Logger.Error("���Ż�ȡ�쳣��", e);
                throw new AbpException("���Ż�ȡ�쳣", e);
            }
        }
        #endregion

        #region ����ɾ������
        
        /// <summary>
        /// ����ɾ������
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
                Logger.Error("����ɾ���쳣��", e);
                throw new AbpException("����ɾ���쳣��", e);
            }
        }
        #endregion

        #region ���Ͷ�����֤��
        /// <summary>
        /// ���Ͷ�����֤�루�����Ȩ��
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
                    throw new AbpException("��������Ϊ�գ�");
                Random rad = new Random();
                int val_code = rad.Next(1000, 10000);
                entity.Code = val_code.ToString();
                string content = "������֤���ǣ�" + val_code + " ���벻Ҫ����֤��й¶�������ˡ�";
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
                    Logger.Error(string.Format("���ŷ���ʧ�ܣ�{0}", myResponse.GetResponseStream()));
                    throw new AbpException(string.Format("���ŷ���ʧ�ܣ�{0}", myResponse.GetResponseStream()));
                }
                await SmsRepository.InsertAsync(entity);
                await CurrentUnitOfWork.SaveChangesAsync();
                return new { ResultCode = entity.ResultCode, ResultMsg = entity.ResultMsg };
            }
            catch (Exception e)
            {
                Logger.Error("���ŷ����쳣��", e);
                throw new AbpException("���ŷ����쳣��", e);
            }
        }
        #endregion
    }

    public class VerificationSmsDto
    {
        /// <summary>
        /// �ֻ�����
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// ������֤��
        /// </summary>
        public string Code { get; set; }
    }

    public class SmsEditDto
    {
        /// <summary>
        /// �ֻ�����
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// ��Чʱ�� Ĭ��5����
        /// </summary>
        //public int? EffectiveTime { get; set; }
    }

    public class SmsDto
    {
        /// <summary>
        /// �ֻ�����
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// ������֤��
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// ���Žӿڷ�����Ϣ
        /// </summary>
        public string ResultMsg { get; set; }
        /// <summary>
        /// ���Žӿڷ��ش���
        /// </summary>
        public string ResultCode { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreationTime { get; internal set; }
    }
}
