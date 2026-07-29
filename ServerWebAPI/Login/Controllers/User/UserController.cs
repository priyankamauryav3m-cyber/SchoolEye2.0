using ApplicationInterface.User;
using DomainModel.User;
using Microsoft.AspNetCore.Mvc;

namespace ServerWebAPI.Login.Controllers.User
{
    [ApiExplorerSettings(GroupName = "Login")]
    //[Authorize]
    [Route("APIUser")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUser User;
        public UserController(IUser _User)
        {
            User = _User;
        }      

        //================================GETUSEDETAILS===============
        [HttpGet]
        [Route("GetUserDetails")]
        public ActionResult<List<UserDetails>> GetUser(string UserTypeId, int HospitalId, int VCID, int IsActive, string LoginName, string GroupCode)
        {
            var ReviewPatientList = User.GetUserDetails(UserTypeId,HospitalId,VCID,IsActive,LoginName,GroupCode);
            return Ok(ReviewPatientList);
        }

    }
}
