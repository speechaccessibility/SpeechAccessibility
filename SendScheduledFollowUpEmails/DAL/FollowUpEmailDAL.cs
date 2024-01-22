using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SendScheduledFollowUpEmails.Model;

namespace SendScheduledFollowUpEmails.DAL
{
    public class FollowUpEmailDAL
    {
        private string _connectionString;
        public FollowUpEmailDAL(IConfiguration iconfiguration)
        {
            _connectionString = iconfiguration.GetConnectionString("Default");
        }

        public List<ScheduledFollowUpEmail> GetScheduledFollowUpEmails()
        {
            var scheduledFollowUpEmails = new List<ScheduledFollowUpEmail>();
            //var recording = new Recording();

            var sql =
                "SELECT   a.Id, a.ContributorId, a.EmailContent, a.ScheduledSendDate, a.SendToContributor, a.SendToHelper, a.SendToMentor,a.MentorEmailAddress, b.FirstName, b.LastName, b.EmailAddress, "
                + "b.HelperEmail, a.SendBy FROM ContributorFollowUp a INNER JOIN v_Contributor b ON a.ContributorId = b.Id WHERE EmailSentDate IS NULL";

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {

                        scheduledFollowUpEmails.Add(new ScheduledFollowUpEmail
                        {
                            Id = Convert.ToInt32(rdr["Id"]),
                            ContributorId = (Guid)rdr["ContributorId"],
                            EmailContent = rdr["EmailContent"].ToString() ?? string.Empty,
                            ScheduledSendDate = (DateTime)rdr["ScheduledSendDate"],
                            SendToContributor = rdr["SendToContributor"].ToString() ?? string.Empty,
                            SendToHelper = rdr["SendToHelper"].ToString() ?? string.Empty,
                            SendToMentor = rdr["SendToMentor"].ToString() ?? string.Empty,
                            MentorEmailAddress = rdr["MentorEmailAddress"].ToString() ?? string.Empty,
                            FirstName = rdr["FirstName"].ToString() ?? string.Empty,
                            LastName = rdr["LastName"].ToString() ?? string.Empty,
                            EmailAddress = rdr["EmailAddress"].ToString() ?? string.Empty,
                            HelperEmail = rdr["HelperEmail"].ToString() ?? string.Empty,
                            SendBy = rdr["SendBy"].ToString() ?? string.Empty

                        });

                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
                //throw ex;
            }
            return scheduledFollowUpEmails;
        }

        public void MarkMessageSent(int id)
        {
            var updateSql = "UPDATE dbo.ContributorFollowUp SET EmailSentDate='" + DateTime.Now + "' WHERE Id=" + id;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(updateSql, con);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }

        public void SaveEmailLogging(Guid contributorId, string subject, string message, string sendTo, string sendBy)
        {
            var insertSql =
                "INSERT INTO dbo.EmailLogging (ContributorId,Subject,Message,SendTo,SendTS,SendBy) VALUES ('" + contributorId + "','" + subject + "','" + message + "','" + sendTo +"','" + DateTime.Now + "','" + sendBy + "')" ;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(insertSql, con);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //throw ex;
            }

        }
    }
}
