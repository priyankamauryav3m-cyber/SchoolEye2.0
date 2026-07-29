using ApplicationInterface.User;

namespace ServerWebAPI.Authorization;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task Invoke(HttpContext context, IUser userRepository, IJwtUtils jwtUtils)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last() ?? context.User.FindFirst("authToken")?.Value; ;
        var userId = jwtUtils.ValidateToken(token);
        if (userId != null)
        {
            
            context.Items["User"] = await userRepository.GetUser(userId);
        }
        await _next(context);
     }
}
    