using System;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace WebApplication.Filters
{
    public class OperationCanceledExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<OperationCanceledExceptionFilter> _logger;

        public OperationCanceledExceptionFilter(ILogger<OperationCanceledExceptionFilter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    
        public override void OnException(ExceptionContext context)
        {
            if (!(context.Exception is OperationCanceledException))
            {
                return;
            }
        
            _logger.LogInformation("Operation was canceled");
        
            /*
             * In order to distinguish between actual client errors and canceled requests in
             * monitoring metrics is a non-standard status code used.
             */
            context.Result = new StatusCodeResult(499);
            context.ExceptionHandled = true;
        }
    }
}
