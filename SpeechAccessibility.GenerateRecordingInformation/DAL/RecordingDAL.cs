using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using MathNet.Numerics.Statistics;
using Microsoft.Extensions.Configuration;
using NAudio.Wave;
using SpeechAccessibility.GenerateRecordingInformation.Model;

namespace SpeechAccessibility.GenerateRecordingInformation.DAL
{
    public class RecordingDAL
    {
        private string _connectionString;
        public RecordingDAL(IConfiguration iconfiguration)
        {
            _connectionString = iconfiguration.GetConnectionString("Default");
        }

        public List<Recording>  GetRecordingInformation()
        {
            var listRecording = new List<Recording>();
            //var recording = new Recording();

            var sql =
                "SELECT Id,FileName,OriginalPromptId,ContributorId,StatusId,BlockId,CreateTS,UpdateTS,LastUpdateBy,StartTime,EndTime FROM Recording WHERE StartTime Is NULL";
            //var sql =
            //    "SELECT Id,FileName,OriginalPromptId,ContributorId,StatusId,BlockId,CreateTS,UpdateTS,LastUpdateBy,StartTime,EndTime FROM Recording WHERE ContributorId='67B8EF26-B2F2-46C2-278F-08DB1B393B1F'";

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                   
                    while (rdr.Read())
                    {
                      
                        listRecording.Add(new Recording
                        {
                            Id = Convert.ToInt32(rdr["Id"]),
                            FileName = rdr["FileName"].ToString() ?? string.Empty,
                            OriginalPromptId = Convert.ToInt32(rdr["OriginalPromptId"]),
                            ContributorId = (Guid)rdr["ContributorId"],
                            BlockId = rdr["BlockId"] is DBNull ? 0 : Convert.ToInt32(rdr["BlockId"])
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
                //throw ex;
            }
            return listRecording;
        }

        public void UpdateRecordingTime(int id, string startTime, string endTime)
        {
            //UpdateRecordingStartEndTime
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("UpdateRecordingStartEndTime", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@StartTime", SqlDbType.VarChar, 10).Value = startTime;
                    cmd.Parameters.Add("@EndTime", SqlDbType.NVarChar, 10).Value = endTime;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Tuple<string, string> GetRecordingTimeSpan(string filePath, sbyte silenceThreshold = -20)
        {
            AudioFileReader reader = new AudioFileReader(filePath);
            var buffer = new float[reader.Length];

            int startPos = 0; // Initial detection of noise meeting threshold
            int endPos = 0; // Final detection of noise meeting threshold
            int samplesRead = reader.Read(buffer, 0, (int)reader.Length);

            // Get threshold base line
            silenceThreshold = GetThresholdBaseline(buffer, samplesRead, silenceThreshold);

            for (int n = 0; n < samplesRead; n++)
                if (!IsSilence(buffer[n], silenceThreshold))
                {
                    if (startPos == 0)
                        startPos = n;
                    endPos = n;
                }
            //since there are audio file that has more than one channel, we need to get the trueSample value
            double trueSample = (double)reader.WaveFormat.SampleRate * reader.WaveFormat.Channels / 1000;
            var startTime = Math.Round((startPos / trueSample) / 100) * 100;
            var endTime = Math.Round((endPos / trueSample) / 100) * 100;

            return new Tuple<string, string>(TimeSpan.FromMilliseconds(startTime).ToString("hh':'mm':'ss'.'f"), TimeSpan.FromMilliseconds(endTime).ToString("hh':'mm':'ss'.'f"));
        }


        public static bool IsSilence(float amplitude, sbyte threshold)
        {
            if (amplitude == 0)
                return true;
            double dB = 20 * Math.Log10(Math.Abs(amplitude));
            return dB < threshold;
        }

        private sbyte GetThresholdBaseline(float[] buffer, int samplesRead, sbyte silenceThreshold)
        {
            List<double> data = new List<double>();
            for (int n = 0; n < samplesRead; n++)
            {
                if (!IsSilence(buffer[n], silenceThreshold))
                    data.Add(Math.Abs(buffer[n]));
            }

            return (sbyte)(20 * Math.Log10(data.Median()));
        }



    }

}

