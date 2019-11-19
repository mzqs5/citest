using Abp;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.Extensions;
using Abp.AspNetCore.Mvc.Results;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Events.Bus;
using Abp.Events.Bus.Exceptions;
using Abp.Logging;
using Abp.Runtime.Validation;
using Abp.UI;
using Abp.Web.Models;
using AutoMapper.Internal;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace IF.Filter
{
    /// <summary>
    /// 
    /// </summary>
    public class ExceptionFilter : IExceptionFilter, ITransientDependency
    {
        /// <summary>
        /// 
        /// </summary>
        public ILogger Logger { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEventBus EventBus { get; set; }

        private readonly IErrorInfoBuilder _errorInfoBuilder;
        private readonly IAbpAspNetCoreConfiguration _configuration;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorInfoBuilder"></param>
        /// <param name="configuration"></param>
        public ExceptionFilter(IErrorInfoBuilder errorInfoBuilder, IAbpAspNetCoreConfiguration configuration)
        {
            _errorInfoBuilder = errorInfoBuilder;
            _configuration = configuration;

            Logger = NullLogger.Instance;
            EventBus = NullEventBus.Instance;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }

            var wrapResultAttribute = new DontWrapResultAttribute();

            if (wrapResultAttribute.LogError)
            {
                LogHelper.LogException(Logger, context.Exception);
            }

            //if (wrapResultAttribute.WrapOnError)
            //{
            HandleAndWrapException(context);
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        private void HandleAndWrapException(ExceptionContext context)
        {
            if (!ActionResultHelper.IsObjectResult(context.ActionDescriptor.GetMethodInfo().ReturnType))
            {
                return;
            }
            ErrorInfo errorInfo = new ErrorInfo();
            errorInfo.Code = GetStatusCode(context);
            errorInfo.Message = context.Exception.Message;
            context.Result = new ObjectResult(
               new { Code = errorInfo.Code, Message = context.Exception.Message, Exception = context.Exception }
            );
            context.HttpContext.Response.StatusCode = errorInfo.Code;

            EventBus.Trigger(this, new AbpHandledExceptionData(context.Exception));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual int GetStatusCode(ExceptionContext context)
        {
            if (context.Exception is AbpAuthorizationException)
            {
                return context.HttpContext.User.Identity.IsAuthenticated
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized;
            }

            if (context.Exception is AbpValidationException || context.Exception is AbpException || context.Exception is UserFriendlyException)
            {
                return (int)HttpStatusCode.BadRequest;
            }

            if (context.Exception is EntityNotFoundException)
            {
                return (int)HttpStatusCode.NotFound;
            }
            return (int)HttpStatusCode.InternalServerError;
        }
    }
}
