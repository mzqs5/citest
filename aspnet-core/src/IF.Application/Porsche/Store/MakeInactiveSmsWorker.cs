using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using IF.Authorization.Users;
using IF.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IF.Porsche
{
    public class MakeInactiveSmsWorker : IFAppServiceBase, IMakeInactiveSmsWorker
    {
        IRepository<AppointmentSmsAggregate> AppointmentSmsRepository;
        IRepository<AppointmentAggregate> AppointmentRepository;
        IRepository<AppointmentTestDriveAggregate> AppointmentTestDriveRepository;
        IRepository<User, long> UserRepository;
        IRepository<CarAggregate> CarRepository;
        IRepository<StoreAggregate> StoreRepository;
        IRepository<SmsAggregate> SmsRepository;
        public MakeInactiveSmsWorker(
            IRepository<AppointmentSmsAggregate> AppointmentSmsRepository,
            IRepository<User, long> UserRepository,
            IRepository<CarAggregate> CarRepository,
            IRepository<StoreAggregate> StoreRepository,
            IRepository<AppointmentAggregate> AppointmentRepository,
            IRepository<SmsAggregate> SmsRepository,
            IRepository<AppointmentTestDriveAggregate> AppointmentTestDriveRepository)

        {
            this.AppointmentSmsRepository = AppointmentSmsRepository;
            this.UserRepository = UserRepository;
            this.CarRepository = CarRepository;
            this.StoreRepository = StoreRepository;
            this.SmsRepository = SmsRepository;
            this.AppointmentRepository = AppointmentRepository;
            this.AppointmentTestDriveRepository = AppointmentTestDriveRepository;
        }

        [UnitOfWork]
        public void DoWork()
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var oneMonthAgo = Clock.Now.Subtract(TimeSpan.FromDays(1));
                var UserCars = UserRepository.GetAll().Include(p => p.UserCars).SelectMany(p => p.UserCars);
                var result = from Appointment in AppointmentRepository.GetAll().AsEnumerable()
                             join UserCar in UserCars
                             on Appointment.UserCarId equals UserCar.Id
                             join Car in CarRepository.GetAll().AsEnumerable()
                             on UserCar.CarId equals Car.Id
                             join Store in StoreRepository.GetAll().AsEnumerable()
                             on Appointment.StoreId equals Store.Id
                             select new AppointmentDto
                             {
                                 Id = Appointment.Id,
                                 Name = Appointment.Name,
                                 Date = Appointment.Date,
                                 Mobile = Appointment.Mobile,
                                 StoreId = Appointment.StoreId,
                                 UserCarId = Appointment.UserCarId,
                                 CarId = UserCar.CarId,
                                 CarName = Car.Name,
                                 FrameNo = UserCar.FrameNo,
                                 StoreName = Store.Name,
                                 StoreAddress = Store.Address,
                                 StorePhone = Store.Phone,
                                 SurName = Appointment.SurName,
                                 UserId = Appointment.UserId,
                                 Sex = Appointment.Sex,
                                 CreationTime = Appointment.CreationTime,
                                 City = Appointment.City,
                                 Area = Appointment.Area,
                                 Street = Appointment.Street,
                                 DetailAddress = Appointment.DetailAddress
                             };
                var list = result.Where(p => p.Date.HasValue && (p.Date.Value - DateTime.Now).Days == 1).ToList();
                list.ForEach(res =>
                {
                    AppointmentSmsAggregate sms = new AppointmentSmsAggregate();
                    sms.UserId = res.UserId;
                    sms.Msg = $"您已预约{res.Date.Value.Year}年{res.Date.Value.Month}月{res.Date.Value.Day}日的维修保养服务，车型 {res.CarName}，追星{res.StoreName}期待您的光临！";
                    AppointmentSmsRepository.Insert(sms);
                    Send(res.Mobile, sms.Msg);
                });
                var lista = result.Where(p => p.Date.HasValue && (p.Date.Value - DateTime.Now).Days == 5).ToList();
                lista.ForEach(res =>
                {
                    Send(res.Mobile, $"您已预约{res.Date.Value.Year}年{res.Date.Value.Month}月{res.Date.Value.Day}日的维修保养服务，车型 {res.CarName}，追星{res.StoreName}期待您的光临！");
                });

                var result1 = from Appointment in AppointmentTestDriveRepository.GetAll().AsEnumerable()
                              join Car in CarRepository.GetAll().AsEnumerable()
                              on Appointment.CarId equals Car.Id
                              join Store in StoreRepository.GetAll().AsEnumerable()
                              on Appointment.StoreId equals Store.Id
                              select new AppointmentTestDriveDto
                              {
                                  Id = Appointment.Id,
                                  Name = Appointment.Name,
                                  Mobile = Appointment.Mobile,
                                  CarId = Appointment.CarId,
                                  CarName = Car.Name,
                                  StoreId = Appointment.StoreId,
                                  StoreName = Store.Name,
                                  StoreAddress = Store.Address,
                                  StorePhone = Store.Phone,
                                  SurName = Appointment.SurName,
                                  UserId = Appointment.UserId,
                                  Sex = Appointment.Sex,
                                  Date = Appointment.Date,
                                  CreationTime = Appointment.CreationTime
                              };

                var list1 = result1.Where(p => p.Date.HasValue && (p.Date.Value - DateTime.Now).Days == 1).ToList();
                list.ForEach(res =>
                {
                    AppointmentSmsAggregate sms = new AppointmentSmsAggregate();
                    sms.UserId = res.UserId;
                    sms.Msg = $"您已预约{res.Date.Value.Year}年{res.Date.Value.Month}月{res.Date.Value.Day}日的试驾服务，车型 {res.CarName}，追星{res.StoreName}期待您的光临！";
                    AppointmentSmsRepository.Insert(sms);
                    Send(res.Mobile, sms.Msg);
                });
                var listb = result1.Where(p => p.Date.HasValue && (p.Date.Value - DateTime.Now).Days == 5).ToList();
                listb.ForEach(res =>
                {
                    Send(res.Mobile, $"您已预约{res.Date.Value.Year}年{res.Date.Value.Month}月{res.Date.Value.Day}日的试驾服务，车型 {res.CarName}，追星{res.StoreName}期待您的光临！");
                });
                CurrentUnitOfWork.SaveChanges();
            }
        }

        private void Send(string Mobile, string content)
        {
            try
            {
                var configuration = AppConfigurations.Get(Directory.GetCurrentDirectory());
                SmsAggregate entity = new SmsAggregate();

                var SmsConfig = configuration.GetSection("Sms");
                if (SmsConfig == null)
                    Logger.Error("短信配置为空！");
                entity.Msg = content;
                entity.Mobile = Mobile;
                string postStrTpl = "account={0}&password={1}&mobile={2}&content={3}";

                UTF8Encoding encoding = new UTF8Encoding();
                byte[] postData = encoding.GetBytes(string.Format(postStrTpl, SmsConfig.GetSection("Account").Value, SmsConfig.GetSection("PassWord").Value, Mobile, content));

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
                }
                SmsRepository.Insert(entity);
                CurrentUnitOfWork.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Error("短信发送异常！", e);
            }
        }
    }
}
