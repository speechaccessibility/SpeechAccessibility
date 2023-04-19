using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Mvc;
using System.Linq;
using MathNet.Numerics.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpeechAccessibility.Annotator.Services;
using SpeechAccessibility.Core.Interfaces;
using NAudio.Wave;
using SpeechAccessibility.Annotator.Extensions;
using System.Configuration;


namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "CompensatorAndAnnotatorAdmin")]
    public class ExtensionController : Controller
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        public ExtensionController(IRoleRepository roleRepository, IConfiguration configuration, IUserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _configuration = configuration;
            _userRepository = userRepository;
        }

        [HttpPost]
        [Authorize(Policy = "AnnotatorAdmin")]
        public ActionResult GetUserInfoFromAD(string netId)
        {
            //check to see if user is already in the system
            var existingUser = _userRepository.Find(u => u.NetId.Equals(netId)).Include(u=>u.Role).FirstOrDefault();
            if (existingUser == null)
            {
                var user = ActiveDirectoryService.GetUserInfoFromAD(netId, _configuration["AppSettings:Domain"]);

                if (user != null && !string.IsNullOrEmpty(user.NetId))
                {
                    return Json(new { Success = true, Message = user });
                }
                return Json(new { Success = false, Message = "NetID is not valid." });
            }

            return Json(new { Success = true, Message = existingUser });

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

        private Tuple<string, string> GetRecordingTimeSpan(string filePath, sbyte silenceThreshold = -20)
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
            double trueSample = (double) reader.WaveFormat.SampleRate * reader.WaveFormat.Channels / 1000;
            var startTime = Math.Round((startPos / trueSample)/100)*100;
            var endTime = Math.Round((endPos / trueSample) / 100) * 100;

            return new Tuple<string, string>(TimeSpan.FromMilliseconds(startTime).ToString("hh':'mm':'ss'.'f"), TimeSpan.FromMilliseconds(endTime).ToString("hh':'mm':'ss'.'f"));
            }

        [HttpPost]
        public ActionResult FileInformation(string filePath)
        {
            //sbyte silenceThreshold = -18; //decibels
            Tuple<string, string> times = GetRecordingTimeSpan(filePath);

            return Json(new
            {
                Success = true,
                Message = "start time: " + times.Item1 + "; end time: " + times.Item2
            });

        }

       

        [HttpGet]
       
        [ServiceFilter(typeof(DeleteFileAttribute))]
        public ActionResult DownloadFile(string fileName)
        {
            
            //Build the File Path.
            var basePath = _configuration["AppSettings:UploadFileFolder"] + "\\GiftCards";
            var fullPath = Path.Combine(basePath, fileName);


            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(fullPath);

            //Send the File to Download.
            return File(bytes, "text/csv", fileName);

           

        }


    }
}
