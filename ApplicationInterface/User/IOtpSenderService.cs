using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.User
{
    public interface IOtpSenderService
    {
        Task SendAsync(string email, string otpCode);

        // one mail with a reset link per user of that email id
        Task SendPasswordResetLinkAsync(string email, IReadOnlyList<(string UserName, string ResetLink)> links, int validMinutes);

    }
}
