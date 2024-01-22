namespace SendScheduledFollowUpEmails.Model
{
    public class ScheduledFollowUpEmail
    {
        public int Id { get; set; }
        public Guid ContributorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string HelperEmail { get; set; }
        public string HelperFirstName { get; set; }
        public string HelperLastName { get; set; }
        public DateTime ScheduledSendDate { get; set; }
        public string SendToContributor { get; set; }
        public string SendToHelper { get; set; }
        public string SendToMentor { get; set; }
        public string MentorEmailAddress { get; set; }
        public string EmailContent { get; set; }
        public  string SendBy { get; set; }
    }
}
