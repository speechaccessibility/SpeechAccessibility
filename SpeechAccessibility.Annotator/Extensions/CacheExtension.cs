using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpeechAccessibility.Annotator.Services;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Extensions
{
    public class CacheExtension
    {
        public static IEnumerable<Prompt> GetPromptList(IPromptRepository promptRepository, int categoryId)
        {
            var cacheKey = "PromptList" + categoryId;
            var promptList = CacheService.Get<IEnumerable<Prompt>>(cacheKey);
            if (promptList != null) return promptList;
            //var promptList = new IEnumerable<Prompt>();
            promptList =  promptRepository.Find(p => p.CategoryId == categoryId && p.Active == "Yes").OrderBy(p => p.Transcript).ToList();
            CacheService.Add(promptList, cacheKey);
            return promptList;
        }
        //public static async Task<IEnumerable<Prompt>> GetPromptList(IPromptRepository promptRepository, int categoryId)
        //{
        //    var cacheKey = "PromptList" + categoryId;
        //    var promptList = CacheService.Get<IEnumerable<Prompt>>(cacheKey);
        //    if (promptList != null) return promptList;
        //    promptList = await promptRepository.Find(p => p.CategoryId== categoryId && p.Active=="Yes").OrderBy(p => p.Transcript);
        //    CacheService.Add(promptList, cacheKey);
        //    return promptList;
        //}

    }
}
