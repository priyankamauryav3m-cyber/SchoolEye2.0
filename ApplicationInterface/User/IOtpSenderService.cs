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

        // fee receipt cancel -> refund to parent: OTP to the parent's email
        Task SendReceiptRefundOtpAsync(string email, string otpCode, string receiptNo, decimal amount, int validMinutes);

        // after the refund: confirmation mail to the parent with the refund receipt PDF attached
        Task SendReceiptRefundConfirmationAsync(string email, string studentName, string receiptNo, decimal amount, DateTime refundDate, byte[] refundReceiptPdf);

    }
}
