using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using SpeechAccessibility.Annotator.Models;
using System.Data;

namespace SpeechAccessibility.Annotator.Services
{
    [Authorize(Policy = "AllAnnotator")]
    public class CallStoredProcedureService
    {
        private static SqlConnection _connection;
    
        public static List<WaitingApprovalContributorViewModel> GetContributorForApproval(string connectionstring)
        {
           
            var querySelect = "SELECT Recording.ContributorId, v_Contributor.FirstName, v_Contributor.LastName " +
                              "FROM Recording INNER JOIN Prompt ON Recording.OriginalPromptId = Prompt.Id " +
                              "INNER JOIN Category ON Prompt.CategoryId = Category.Id INNER JOIN " +
                              "v_Contributor ON Recording.ContributorId = v_Contributor.Id " +
                              "GROUP BY Recording.ContributorId, Category.Id, v_Contributor.FirstName, v_Contributor.LastName,v_Contributor.IsApproved " +
                              "HAVING(Category.Id = 1 AND v_Contributor.IsApproved Is NULL)";
            var contributorList = new List<WaitingApprovalContributorViewModel>();
            using (_connection = new SqlConnection(connectionstring))
            {
                var cmd = new SqlCommand(querySelect, _connection);

                using (cmd)
                {
                    cmd.Connection.Open();
                    var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        var contributor = new WaitingApprovalContributorViewModel
                        {
                            ContributorId = (Guid)rd["ContributorId"],
                            FirstName = rd["FirstName"].ToString(),
                            LastName = rd["LastName"].ToString()

                        };

                        contributorList.Add(contributor);
                    }
                }
                return contributorList;
            }
        }
        public static void ApproveDenyContributor(string connectionstring, Guid contributorId, string comments, int action)
        {
            using (_connection = new SqlConnection(connectionstring))
            {
                using (var cmd = new SqlCommand("ApproveDenyContributor", _connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ContributorId", contributorId));
                    cmd.Parameters.Add(new SqlParameter("@Action", action));
                    cmd.Parameters.Add(new SqlParameter("@Comments", !string.IsNullOrEmpty(comments)?comments:""));
                    cmd.Connection.Open();
                    cmd.ExecuteNonQuery();
                }

            }

        }


    }
}
