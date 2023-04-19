using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace SpeechAccessibility.Annotator.Extensions
{
    public class DeleteFileAttribute : ActionFilterAttribute
    {

        private readonly IConfiguration _configuration;

        public DeleteFileAttribute(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //filterContext.HttpContext.Response.Flush();

         


            //convert the current filter context to file and get the file path
            var filePath = _configuration["AppSettings:UploadFileFolder"] + "\\GiftCards\\" + (filterContext.Result as FileContentResult).FileDownloadName;
           

            //delete the file after download
            File.Delete(filePath);
        }
        
    }
}
