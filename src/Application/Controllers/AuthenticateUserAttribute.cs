using System;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace KidsTown.Application.Controllers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthenticateUserAttribute : Attribute, IAsyncActionFilter
{
    private const string UserHeaderName = "us";
    private const string PasswordHeaderName = "pw";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(UserHeaderName, out var extractedUser))
        {
            context.Result = new ContentResult {StatusCode = 401, Content = "Auth not provided"};
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(
                PasswordHeaderName,
                out var extractedPassword))
        {
            context.Result = new ContentResult {StatusCode = 401, Content = "Auth not provided"};
            return;
        }

        var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

        var isValidLogin = await userRepository.IsValidLogin(extractedUser!, extractedPassword!);

        if (!isValidLogin)
        {
            context.Result = new ContentResult {StatusCode = 401, Content = "Auth is not valid"};
            return;
        }

        await next();
    }
}