using Microsoft.Extensions.Configuration;
using SpeechAccessibility.GenerateRecordingInformation.DAL;

// See https://aka.ms/new-console-template for more information
var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsetings.json", false, true);

var config = builder.Build();
var recordingDAL = new RecordingDAL(config);
var recordings = recordingDAL.GetRecordingInformation();
var filePath = "";

recordings.ForEach(recording =>
{
    //path: ContributorId\BlockId\modified\filename
    string path = config["AppSettings:UploadFileFolder"] + "\\" + recording.ContributorId + "\\" + recording.BlockId + "\\modified\\" + recording.FileName;
    if (File.Exists(path))
    {
        Tuple<string, string> times = recordingDAL.GetRecordingTimeSpan(path);
        recordingDAL.UpdateRecordingTime(recording.Id,times.Item1.ToString(), times.Item2.ToString());

    }
});



