using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Mvc;
using System.Linq;
using MathNet.Numerics.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpeechAccessibility.Annotator.Services;
using SpeechAccessibility.Core.Interfaces;
using NAudio.Wave;
using SpeechAccessibility.Annotator.Extensions;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Core.Models;


namespace SpeechAccessibility.Annotator.Controllers
{
    [Authorize(Policy = "CompensatorAndAnnotatorAdmin")]
    public class ExtensionController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IUserSubRoleRepository _userSubRoleRepository;
        private readonly ISubRoleRepository _subRoleRepository;
        public ExtensionController(IConfiguration configuration, IUserRepository userRepository, IUserSubRoleRepository userSubRoleRepository, ISubRoleRepository subRoleRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _userSubRoleRepository = userSubRoleRepository;
            _subRoleRepository = subRoleRepository;
        }

        [HttpPost]
        [Authorize(Policy = "AnnotatorAdmin")]
        public ActionResult GetUserInfoFromAD(string netId)
        {
            //check to see if user is already in the system
            var existingUser = _userRepository.Find(u => u.NetId.Equals(netId)).Include(u=>u.Role).FirstOrDefault();
            var user = new ADMemberViewModel();
            if (existingUser == null)
            {
                user = ActiveDirectoryService.GetUserInfoFromAD(netId, _configuration["AppSettings:Domain"]);

                if (user != null && !string.IsNullOrEmpty(user.NetId))
                {
                    return Json(new { Success = true, Message = user });
                }
                return Json(new { Success = false, Message = "NetID is not valid." });
            }


            user.Id = existingUser.Id;
            user.NetId = existingUser.NetId;
            user.RoleId= existingUser.RoleId;
            user.HasSubRole = existingUser.Role.HasSubRole;
            user.LastName = existingUser.LastName;
            user.FirstName=existingUser.FirstName;
            user.Active = existingUser.Active;
            user.AvailableSubRoles = _subRoleRepository.Find(r => r.RoleId == existingUser.RoleId)
                .Include(r => r.Etiology)
                //.Select(a => new SelectListItem()
                //{
                //    Value = a.Etiology.Id.ToString(),
                //    Text = a.Etiology.Name
                //})
                //.ToList();
                .Select(s => new SubRole() { Id = s.EtiologyId })
                .ToList();

            //get user's subrole
            if (existingUser.Role.HasSubRole)
            {
                user.AssignedSubRoles = _userSubRoleRepository.Find(s => s.UserId == existingUser.Id)
                    //.Select(a => new SelectListItem()
                    //{
                    //    Value = a.SubRole.Etiology.Id.ToString(),
                    //    Text = a.SubRole.Etiology.Name
                    //})
                    //.ToList();
                    .Select(s => new SubRole() { Id = s.SubRole.EtiologyId }).ToList(); 
            }
            return Json(new { Success = true, Message = user });

        }

        [HttpPost]
        [Authorize(Policy = "AnnotatorAdmin")]
        public ActionResult GetAvailableSubRoles(int roleId)
        {
            var subRoles = _subRoleRepository.Find(r => r.RoleId == roleId && r.Etiology.Active=="Yes").Include(r=>r.Etiology)
                .Select(s => new Etiology() { Id = s.EtiologyId })
                //.Select(s => new SubRole() { Id = s.Id })
                .ToList();

            return Json(new { Success = true, Message = subRoles });

        }

        //public bool HasSpeech(byte[] buffer)
        //{
        //    using (var vad = new WebRtcVad())
        //    {
        //        vad.OperatingMode = OperatingMode.VeryAggressive;
        //        var hasSpeech = vad.HasSpeech(buffer, SampleRate.Is48kHz, FrameLength.Is30ms);
        //        return hasSpeech;
        //    }
        //}

        //[HttpPost]
        //public ActionResult FileInformation(string filePath)
        //{
        //    var tempPath = "";

        //    AudioFileReader reader = new AudioFileReader(filePath);
        //    var buffers = new float[reader.Length];
        //    int samplesRead = reader.Read(buffers, 0, (int)reader.Length);
        //    int bps = reader.WaveFormat.BitsPerSample;


        //    using (var rdr = new WaveFileReader(filePath))
        //    {
        //        var monoWaveForamt = WaveFormat.CreateCustomFormat(rdr.WaveFormat.Encoding,
        //            rdr.WaveFormat.SampleRate,
        //            1,
        //            rdr.WaveFormat.AverageBytesPerSecond, rdr.WaveFormat.BlockAlign,
        //            rdr.WaveFormat.BitsPerSample);

        //        using (var writer = new WaveFileWriter(tempPath, monoWaveForamt))
        //        {
        //            var stBuffer = new byte[2 * rdr.WaveFormat.BlockAlign];

        //            while (rdr.Read(stBuffer, 0, stBuffer.Length) > 0)
        //            {
        //                var lSamp = BitConverter.ToInt16(stBuffer, 0);
        //                var rSamp = BitConverter.ToInt16(stBuffer, 2);
        //                var mSamp = (short)((lSamp + rSamp) / 2);

        //                writer.Write(BitConverter.GetBytes(mSamp), 0, 2);
        //                writer.Write(BitConverter.GetBytes(mSamp), 0, 2);
        //            }

        //        }
        //    }

        //    var frameLength = 30;
        //    var cntr = 0;
        //    var start = -1.0;
        //    var end = -1.0;
        //    var tots = 0;

        //    using (var rdr = new WaveFileReader(filePath))
        //    {
        //        var sRate = rdr.WaveFormat.SampleRate;
        //        var bytesPerSample = rdr.WaveFormat.BitsPerSample / 8;
        //        var channels = rdr.WaveFormat.Channels;
        //        var chunk = sRate * bytesPerSample * channels * frameLength / 1000;

        //        var buff = new byte[chunk];

        //        while (rdr.Read(buff, 0, buff.Length) > 0)
        //        {
        //            tots++;
        //            var isSpeech = HasSpeech(buff);

        //            if (isSpeech)
        //            {
        //                cntr++;
        //            }
        //            else
        //            {
        //                cntr = 0;
        //            }

        //            if (start < 0)
        //            {
        //                if (cntr > 3)
        //                {
        //                    start = tots;
        //                    end = tots;
        //                    cntr = 0;
        //                }
        //            }
        //            else
        //            {
        //                if (cntr > 3)
        //                {
        //                    end = tots;
        //                    cntr = 0;
        //                }
        //            }
        //        }
        //    }

        //    return Json(new
        //    {
        //        Success = true,
        //        Message = $"start: {(start-3)*.03:F2}, end: {end*.03:F2}"
        //    });



        //}

        public static bool IsSilence(float amplitude, sbyte threshold)
        {
            if (amplitude == 0)
                return true;
            double dB = 20 * Math.Log10(Math.Abs(amplitude));
            return dB < threshold;
        }

        private sbyte GetThresholdBaseline(float[] buffer, int samplesRead, sbyte silenceThreshold)
        {
            //silenceThreshold = -20;
            List<double> data = new List<double>();
            for (int n = 0; n < samplesRead; n++)
            {
                if (!IsSilence(buffer[n], silenceThreshold))
                    data.Add(Math.Abs(buffer[n]));
            }
            return (sbyte)(20 * Math.Log10(data.Median()));
        }

        private Tuple<string, string> GetRecordingTimeSpan(string filePath, sbyte minThreshold = -40, sbyte maxThreshold = -60, sbyte incThreshold = -10, int series = 2 )
        {
            var path = "\\\\bi-isilon-smb.beckman.illinois.edu\\NovaH\\Dev\\Bic\\";
            filePath = path + filePath;
            AudioFileReader reader = new AudioFileReader(filePath);
            var buffer = new float[reader.Length];

            int startPos = 0; // Initial detection of noise meeting threshold
            int endPos = 0; // Final detection of noise meeting threshold
            int samplesRead = reader.Read(buffer, 0, (int)reader.Length);

            sbyte currentThreshold = minThreshold;

            // Get threshold base line
            do
            {
                minThreshold = GetThresholdBaseline(buffer, samplesRead, currentThreshold);

                if (minThreshold != 0)
                    break;
                currentThreshold = (sbyte)(currentThreshold + incThreshold);
            } while (currentThreshold >= maxThreshold);


            int cycles = 0;

            for (int n = 0; n < samplesRead; n++)
                if (!IsSilence(buffer[n], minThreshold))
                {
                    cycles++;
                    if (cycles > series)
                    {
                        if (startPos == 0)
                            startPos = n - series;
                        endPos = n;
                    }
                }
                else
                {
                    cycles = 0;
                }

            //since there are audio file that has more than one channel, we need to get the trueSample value
            double trueSample = (double)reader.WaveFormat.SampleRate * reader.WaveFormat.Channels / 1000;
            var startTime = Math.Round((startPos / trueSample) / 10) * 10;
            var endTime = Math.Round((endPos / trueSample) / 10) * 10;

            return new Tuple<string, string>(TimeSpan.FromMilliseconds(startTime).ToString("hh':'mm':'ss'.'ff"), TimeSpan.FromMilliseconds(endTime).ToString("hh':'mm':'ss'.'ff"));
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
