using Abp;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using DevExtreme.AspNet.Mvc;
using IF.Authorization.Accounts;
using IF.Authorization.Users;
using IF.Porsche;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IF.Authorization
{
    public class UserAccountAppService : IFAppServiceBase, IUserAccountAppService
    {
        IRepository<User, long> UserRepository;
        IRepository<OrderAggregate> OrderRepository;
        IRepository<StoreAggregate> StoreRepository;
        UserManager userManager;
        public UserAccountAppService(
            IRepository<User, long> UserRepository,
            IRepository<OrderAggregate> OrderRepository,
            IRepository<StoreAggregate> StoreRepository,
            UserManager userManager)
        {
            this.userManager = userManager;
            this.StoreRepository = StoreRepository;
            this.UserRepository = UserRepository;
            this.OrderRepository = OrderRepository;
        }

        /// <summary>
        /// ��ȡ�û���Ϣ�������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from user in UserRepository.GetAll().Include(p => p.Roles).Where(p => p.Roles.Select(r => r.RoleId).Contains(3)).Include(p => p.Contacts).Include(p => p.UserCars).Include(p => p.Wechats).Include(p => p.Address).AsEnumerable()
                         join Store in StoreRepository.GetAll().AsEnumerable()
                         on user.LastStoreId equals Store.Id
                         into Stores
                         from Store in Stores.DefaultIfEmpty()
                         select new UserAccountDto
                         {
                             Id = user.Id,
                             Name = user.Name,
                             UserName = user.UserName,
                             Surname = user.Surname,
                             Mobile = user.Mobile,
                             Sex = user.Sex,
                             IsCarOwner = user.IsCarOwner,
                             LastStoreId = user.LastStoreId,
                             StoreName = Store == null ? "" : Store.Name,
                             StoreId = user.StoreId,
                             CreationTime = user.CreationTime,
                             Contacts = this.ObjectMapper.Map<List<ContactsDto>>(user.Contacts),
                             UserCars = this.ObjectMapper.Map<List<UserCarDto>>(user.UserCars),
                             Wechats = this.ObjectMapper.Map<List<WechatDto>>(user.Wechats),
                             Address = this.ObjectMapper.Map<List<AddressDto>>(user.Address)
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region ����
        [HttpGet]
        [AbpAuthorize("Admin.Members.Export")]
        public async Task<IActionResult> Export(DataSourceLoadOptions loadOptions)
        {
            var result = from user in UserRepository.GetAll().Include(p => p.Roles).Where(p => p.Roles.Select(r => r.RoleId).Contains(3)).Include(p => p.Contacts).Include(p => p.UserCars).Include(p => p.Wechats).Include(p => p.Address).AsEnumerable()
                         select new UserAccountDto
                         {
                             Id = user.Id,
                             Name = user.Name,
                             UserName = user.UserName,
                             Surname = user.Surname,
                             Mobile = user.Mobile,
                             Contacts = this.ObjectMapper.Map<List<ContactsDto>>(user.Contacts),
                             UserCars = this.ObjectMapper.Map<List<UserCarDto>>(user.UserCars),
                             Wechats = this.ObjectMapper.Map<List<WechatDto>>(user.Wechats),
                             Address = this.ObjectMapper.Map<List<AddressDto>>(user.Address)
                         };
            var Export = new ExportHelper();
            Export.AddColumn("UserName", "�û���");
            Export.AddColumn("Surname", "��");
            Export.AddColumn("Name", "��");
            Export.AddColumn("Mobile", "�ֻ�����");
            Export.AddColumn("Contacts", "��ϵ��");
            Export.AddColumn("UserCars", "�û�����");
            Export.AddColumn("Wechats", "΢����Ϣ");
            Export.AddColumn("Address", "�û���ַ");
            var result1 = Export.ExportExcel<UserAccountDto>(base.DataSourceLoadMap(result, loadOptions).data);
            result1.FileDownloadName = "��Ա��¼";
            return result1;
        }

        #endregion

        #region ����ID��ȡ�û���Ϣ��Ϣ
        /// <summary>
        /// ����ID��ȡ�û���Ϣ��Ϣ
        /// </summary>
        /// <param name="id">�û���Ϣ��Ϣ����</param>
        /// <returns></returns>
        public async Task<UserAccountDto> GetAsync(long id)
        {
            try
            {
                var user = await UserRepository.GetAll().Include(p=>p.Wechats).Include(p=>p.Contacts).Include(p=>p.Address).Include(p=>p.UserCars).Include(p=>p.UserCModels).Where(k => k.Id == id).FirstOrDefaultAsync();

                if (user == null)
                {
                    throw new EntityNotFoundException(typeof(User), id);
                }
                var dto = this.ObjectMapper.Map<UserAccountDto>(user);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("�û���Ϣ��ȡ�쳣��", e);
                throw new AbpException("�û���Ϣ��ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸����û���Ϣ��Ϣ
        /// <summary>
        /// �������߸����û���Ϣ��Ϣ
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.Members.Edit")]
        public async Task SaveAsync(UserAccountEditDto input)
        {
            try
            {
                User entity = this.ObjectMapper.Map<User>(input);
                if (input.Id != 0)
                {
                    User user = UserRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    user.Name = entity.Name;
                    user.Surname = entity.Surname;
                    user.Mobile = entity.Mobile;
                    await UserRepository.UpdateAsync(user);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Error("�û���Ϣ�����쳣��", e);
                throw new AbpException("�û���Ϣ�����쳣��", e);
            }
        }
        #endregion

        #region  �����û��������ܺ�
        /// <summary>
        /// �����û��������ܺ�
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.Members.Edit")]
        public async Task SaveUserCarAsync(UpdateUserCarEditDto input)
        {
            try
            {
                User user = UserRepository.GetAll().Include(p => p.UserCars).Where(k => k.Id == input.Id).FirstOrDefault();
                var userCar = user.UserCars.FirstOrDefault(p => p.Id == input.UserCarId);
                if (userCar == null)
                    throw new AbpException("δ�ҵ����û��ĳ���,���������ѷ����仯��");
                userCar.FrameNo = input.FrameNo;
                await UserRepository.UpdateAsync(user);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("�������ܺű����쳣��", e);
                throw new AbpException("�������ܺű����쳣��", e);
            }
        }
        #endregion

        #region ����ɾ���û���Ϣ
        /// <summary>
        /// ����ɾ���û���Ϣ
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.Members.Delete")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    if (OrderRepository.GetAll().Any(p => p.UserId == int.Parse(id)))
                        throw new AbpException("���û����ڶ���������ɾ����");

                    await UserRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (AbpException e)
            {
                Logger.Error("�û���Ϣɾ���쳣��", e);
                throw e;
            }
        }
        #endregion
        
    }

    public class UpdateUserCarEditDto : EntityDto<long>
    {
        /// <summary>
        /// ���ܺ�
        /// </summary>
        public string FrameNo { get; set; }

        public int UserCarId { get; set; }
    }

    public class UserAccountEditDto : EntityDto<long>
    {
        /// <summary>
        /// ��
        /// </summary>
        public new string Name { get; set; }
        /// <summary>
        /// ��
        /// </summary>
        public new string Surname { get; set; }

        /// <summary>
        /// �Ա� 0�� 1Ů
        /// </summary>
        public int Sex { get; set; }
        /// <summary>
        /// �Ƿ��� 0�� 1��
        /// </summary>
        public int IsCarOwner { get; set; }
        /// <summary>
        /// �ֻ���
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// �����ŵ�
        /// </summary>
        public int LastStoreId { get; set; }
    }

    public class UserAccountDto : EntityDto<long>
    {
        public string UserName { get; set; }
        public string Name { get; set; }

        public string Surname { get; set; }
        public string FullName { get { return (this.Surname + " " + this.Name); } }

        public int Sex { get; set; }

        public int IsCarOwner { get; set; }
        public string Mobile { get; set; }

        public string UnionId { get; set; }

        public int LastStoreId { get; set; }

        public string StoreName { get; set; }
        public int StoreId { get; set; }

        public List<WechatDto> Wechats { get; set; }

        public List<UserCarDto> UserCars { get; set; }

        public List<UserCModelDto> UserCModels { get; set; }

        public List<ContactsDto> Contacts { get; set; }

        public List<AddressDto> Address { get; set; }
        public DateTime CreationTime { get; set; }
    }
}

