
namespace DomainModel.Common
{
    public class DMBugTracker
    {
        public string BugModule { get; set; } = default!;
        public string BugPage { get; set; } = default!;
        public string BugMethod { get; set; } = default!;
        public string ControlerRouteName { get; set; } = default!;
        public string BugMessage { get; set; } = default!;
        public string GroupId { get; set; } = default!;
        public string HospitalId { get; set; } = default!;
        public string VCId { get; set; } = default!;
        public DateTime BugDateTime { get; set; } = default!;
        public string BugUserId { get; set; } = default!;
    }
}
