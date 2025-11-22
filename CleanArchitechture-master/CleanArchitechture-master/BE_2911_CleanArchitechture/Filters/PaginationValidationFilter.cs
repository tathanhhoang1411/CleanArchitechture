using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CleanArchitecture.Application.Utilities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BE_2911_CleanArchitechture.Filters
{
    public class PaginationValidationFilter : IAsyncActionFilter
    {
        private readonly IConfiguration _configuration;

        public PaginationValidationFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var query = context.HttpContext.Request.Query;

            // If no pagination params provided, continue
            if (!query.ContainsKey("skip") && !query.ContainsKey("take"))
            {
                await next();
                return;
            }

            int skip = 0;
            int take = 0;

            if (query.ContainsKey("skip") && !int.TryParse(query["skip"], out skip))
            {
                var errors = new List<string> { "Query parameter 'skip' ph?i là s? nguyên." };
                context.Result = new BadRequestObjectResult(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                return;
            }

            if (query.ContainsKey("take") && !int.TryParse(query["take"], out take))
            {
                var errors = new List<string> { "Query parameter 'take' ph?i là s? nguyên." };
                context.Result = new BadRequestObjectResult(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                return;
            }

            if (skip < 0)
            {
                var errors = new List<string> { "Query parameter 'skip' không ðý?c âm." };
                context.Result = new BadRequestObjectResult(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                return;
            }

            if (take <= 0)
            {
                var errors = new List<string> { "Query parameter 'take' ph?i l?n hõn 0." };
                context.Result = new BadRequestObjectResult(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                return;
            }

            var configuredMaxTake = _configuration.GetValue<int?>("Paging:MaxTake");
            var maxTake = (configuredMaxTake.HasValue && configuredMaxTake.Value > 0) ? configuredMaxTake.Value : 1000;

            if (take > maxTake)
            {
                var errors = new List<string> { $"Parameter 'take' không ðý?c l?n hõn {maxTake}. Vui l?ng gi?m giá tr? 'take' ho?c s? d?ng phân trang (skip). Ví d? m?u: ?skip=0&take=50" };
                context.Result = new BadRequestObjectResult(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                return;
            }

            await next();
        }
    }
}
