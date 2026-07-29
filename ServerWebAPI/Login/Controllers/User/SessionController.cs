using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerWebAPI.Authorization;

namespace ServerWebAPI.Login.Controllers.User
{
    [ApiExplorerSettings(GroupName = "Login")]
    [ApiController]
    [Route("api/session")]
    public class SessionController : ControllerBase
    {
        private readonly SessionManager _sessionManager;
        private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(5);

        public SessionController(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }
        [HttpPost("start")]
        public IActionResult StartSession([FromQuery] string sessionId)
        {
            _sessionManager.UpdateActivity(sessionId);
            return Ok(new { message = "Session started" });
        }

        [HttpPost("ping")]
        public IActionResult Ping([FromQuery] string sessionId)
        {
            _sessionManager.UpdateActivity(sessionId);
            return Ok(new { message = "Session refreshed" });
        }

        [HttpGet("check")]
        public IActionResult CheckSession([FromQuery] string sessionId)
        {
            if (_sessionManager.IsSessionActive(sessionId, SessionTimeout))
                return Ok(new { active = true });

            return Unauthorized(new { active = false });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromQuery] string sessionId)
        {
            _sessionManager.RemoveSession(sessionId);
            return Ok(new { message = "Session removed" });
        }
    }
}
