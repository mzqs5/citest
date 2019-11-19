//using Abp.Configuration.Startup;
//using Abp.Domain.Uow;
//using IF.Configuration;
//using IF.Web;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace IF.EntityFrameworkCore
//{
//    public class MyConnectionStringResolver : DefaultConnectionStringResolver
//    {
//        public MyConnectionStringResolver(IAbpStartupConfiguration configuration)
//            : base(configuration)
//        {
//        }

//        public override string GetNameOrConnectionString(ConnectionStringResolveArgs args)
//        {
//            var connectStringName = this.GetConnectionStringName(args);
//            if (connectStringName != null)
//            {
//                var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());
//                return configuration.GetConnectionString(connectStringName);
//            }
//            return base.GetNameOrConnectionString(args);
//        }
//        private string GetConnectionStringName(ConnectionStringResolveArgs args)
//        {
//            var type = args["DbContextConcreteType"] as Type;
//            if (type == typeof(OutSideDbContext))
//            {
//                return IFConsts.OutSideConnectionStringName;//返回数据库二的节点名称
//            }
//            return null;//采用默认数据库
//        }
//    }

//}
