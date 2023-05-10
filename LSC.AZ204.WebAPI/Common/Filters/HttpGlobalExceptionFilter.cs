using LSC.AZ204.WebAPI.Common.Filters.ActionResults;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Net;

namespace LSC.AZ204.WebAPI.Common.Filters
{
    /// <summary>
    /// Global Exception Handler
    /// </summary>
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment env;
        private readonly ILogger<HttpGlobalExceptionFilter> logger;

        /// <summary>
        /// Global Exception Handler constructor
        /// </summary>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
        {
            this.env = env;
            this.logger = logger;
        }
        /// <summary>
        /// this is used to be  Call after an action has thrown an System.Exception.
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            logger.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception.Message);

            // This is often very handy information for tracing the specific request
            var traceId = Activity.Current?.Id ?? context.HttpContext?.TraceIdentifier;

            var json = new JsonErrorResponse
            {
                Messages = new[] { "An error ocurred." },
                TraceId = traceId ?? string.Empty
            };

            //if (env.IsDevelopment())
            //{
            json.DeveloperMessage = context.Exception;
            //}

            context.Result = new InternalServerErrorObjectResult(json);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.ExceptionHandled = true;
        }

        private class JsonErrorResponse
        {
            public string[] Messages { get; set; }
            public string TraceId { get; set; }
            public object DeveloperMessage { get; set; }
        }
    }
}
