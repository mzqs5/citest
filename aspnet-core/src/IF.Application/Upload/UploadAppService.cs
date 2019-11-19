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
using Abp.Authorization;
using IF.Authorization;
using Microsoft.AspNetCore.Http;
using Abp.IO.Extensions;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using IF.Configuration;

namespace IF.Porsche
{
    public class UploadAppService : IFAppServiceBase, IUploadAppService
    {
        IHostingEnvironment hostingEnv;
        IRepository<FileAggregate> FileRepository;
        IHttpContextAccessor HttpContextAccessor;
        public UploadAppService(
            IRepository<FileAggregate> FileRepository,
            IHttpContextAccessor HttpContextAccessor,
            IHostingEnvironment env)
        {
            this.FileRepository = FileRepository;
            this.HttpContextAccessor = HttpContextAccessor;
            this.hostingEnv = env;
        }
        /// <summary>
        /// ��ȡ�ļ��������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from File in FileRepository.GetAll().AsEnumerable()
                         select new FileDto
                         {
                             FilePath = File.FilePath,
                             FileName = File.FileName,
                             FileSize = File.FileSize,
                             HostAddress = File.HostAddress,
                             Id = File.Id
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region ����ID��ȡ�ļ���Ϣ
        /// <summary>
        /// ����ID��ȡ�ļ���Ϣ
        /// </summary>
        /// <param name="id">�ļ���Ϣ����</param>
        /// <returns></returns>
        public async Task<FileDto> GetAsync(int id)
        {
            try
            {
                var File = await FileRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (File == null)
                {
                    throw new EntityNotFoundException(typeof(FileAggregate), id);
                }
                var dto = this.ObjectMapper.Map<FileDto>(File);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("�ļ���ȡ�쳣��", e);
                throw new AbpException("�ļ���ȡ�쳣", e);
            }
        }
        #endregion

        #region  �ϴ��ļ�
        /// <summary>
        /// �ϴ��ļ�
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestSizeLimit(1_074_790_400)]
        public async Task<object> UploadFilesAsync(IFormFileCollection files)
        {
            HttpContext context = HttpContextAccessor.HttpContext;
            var uploadFiles = files.Count > 0 ? files : context.Request.Form.Files;
            long size = uploadFiles.Sum(f => f.Length);
            if (size > 104857600)
                throw new AbpException("files total size > 100MB , server refused !");

            List<FileAggregate> uploadFileInfos = new List<FileAggregate>();
            try
            {
                foreach (var file in uploadFiles)
                {
                    FileAggregate fileAggregate = new FileAggregate();
                    fileAggregate.FileSize = file.Length;
                    fileAggregate.FileName = file.FileName;
                    fileAggregate.FileBuffer = file.OpenReadStream().GetAllBytes();

                    fileAggregate.HostAddress = context.Request.Host.Value;
                    
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

                    string filePath = hostingEnv.WebRootPath + $@"\files\";
                    fileAggregate.FileName = Guid.NewGuid() + "." + fileName.Split('.')[1];
                    string fileFullName = filePath + fileAggregate.FileName;
                    using (FileStream fs = File.Create(fileFullName))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }

                    var configuration = AppConfigurations.Get(Directory.GetCurrentDirectory());
                    fileAggregate.FilePath = configuration.GetSection("FileService").Value + $"/files/{fileAggregate.FileName}";
                    await FileRepository.InsertAsync(fileAggregate);
                    uploadFileInfos.Add(fileAggregate);
                }
                return this.ObjectMapper.Map<List<FileDto>>(uploadFileInfos);
            }
            catch (Exception e)
            {
                Logger.Error("�ļ��ϴ�ʧ�ܣ�", e);
                throw new AbpException("�ļ��ϴ�ʧ�ܣ�", e);
            }
        }
        #endregion

        #region ����ɾ���ļ�
        /// <summary>
        /// ����ɾ���ļ�
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await FileRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("�ļ�ɾ���쳣��", e);
                throw new AbpException("�ļ�ɾ���쳣��", e);
            }
        }
        #endregion
    }

    public class FileDto : EntityDto
    {
        /// <summary>
        /// �ļ���ַ
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// �ļ�����
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// �ļ���С
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// �ϴ�������ַ
        /// </summary>
        public string HostAddress { get; set; }
    }
}
