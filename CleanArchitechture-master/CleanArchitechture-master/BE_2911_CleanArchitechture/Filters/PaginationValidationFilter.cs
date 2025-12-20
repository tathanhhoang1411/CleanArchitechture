using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CleanArchitecture.Application.Utilities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;

namespace BE_2911_CleanArchitechture.Filters
{
    public class PaginationValidationFilter : IAsyncActionFilter
    {
        private readonly IConfiguration _configuration;
        private readonly int _maxTake;

        public PaginationValidationFilter(IConfiguration configuration)
        {
            _configuration = configuration;
            var configuredMaxTake = _configuration.GetValue<int?>("Paging:MaxTake");
            _maxTake = (configuredMaxTake.HasValue && configuredMaxTake.Value > 0) ? configuredMaxTake.Value : 1000;
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

            bool hasSkip = query.ContainsKey("skip");
            bool hasTake = query.ContainsKey("take");

            int skip = 0;
            int take = 0;

            bool ParseInvariant(string input, out int value) => int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

            if (hasSkip && !ParseInvariant(query["skip"], out skip))
            {
                var errors = new List<string> { "Query parameter 'skip' ph?i là s? nguyên." };
                context.Result = new BadRequestObjectResult(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                return;
            }

            if (hasTake && !ParseInvariant(query["take"], out take))
            {
                var errors = new List<string> { "Query parameter 'take' ph?i là s? nguyên." };
                context.Result = new BadRequestObjectResult(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                return;
            }

            if (hasSkip && skip < 0)
            {
                var errors = new List<string> { "Query parameter 'skip' không ðý?c âm." };
                context.Result = new BadRequestObjectResult(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                return;
            }

            if (hasTake && take <= 0)
            {
                var errors = new List<string> { "Query parameter 'take' ph?i l?n hõn 0." };
                context.Result = new BadRequestObjectResult(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                return;
            }

            if (hasTake && take > _maxTake)
            {
                var errors = new List<string> { $"Parameter 'take' không ðý?c l?n hõn {_maxTake}. Vui l?ng gi?m giá tr? 'take' ho?c s? d?ng phân trang (skip). Ví d?: ?skip=0&take=50" };
                context.Result = new BadRequestObjectResult(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                return;
            }

            await next();
        }
    }
}
