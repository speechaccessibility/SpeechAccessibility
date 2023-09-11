using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SpeechAccessibility.Annotator.Services;
using SpeechAccessibility.Core.Interfaces;
using SpeechAccessibility.Core.Models;
using SpeechAccessibility.Infrastructure.Data;

namespace SpeechAccessibility.Annotator.Extensions
{
    public class CacheExtension
    {
        //public static List<Etiology> GetEtiologyList(IEtiologyRepository etiologyRepository)
        public static List<Etiology> GetEtiologyList(ISubRoleRepository subRoleRepository)
        {
            var cacheKey = "Etiologies";
            var etiologies = CacheService.Get<List<Etiology>>(cacheKey);
            if (etiologies != null)
            {
                return etiologies;
            }

            //etiologies = etiologyRepository.Find(e => e.Active == "Yes").OrderBy(r => r.DisplayOrder).ToList();
            etiologies = subRoleRepository.Find(r => r.InUsed=="Yes" && r.Etiology.Active=="Yes")
                 //.GroupBy(s => s.EtiologyId)
                 // .Select(g => g.First())
                 .Select(s => s.Etiology).Distinct()
                 .ToList();
            CacheService.Add(etiologies, cacheKey);
            return etiologies;
        }

        public static List<UserSubRole> GetSubRoles(IUserSubRoleRepository userSubRoleRepository, string netId)
        {
            var cacheKey = "UserSubRole";
            var userSubRoles = CacheService.Get<List<UserSubRole>>(cacheKey);
            if (userSubRoles != null)
            {
                return userSubRoles;
            }

           var temp = userSubRoleRepository.Find(r => r.User.NetId == netId).Include(r => r.SubRole)
                .ThenInclude(sub => sub.Etiology);

            userSubRoles = userSubRoleRepository.Find(r=>r.User.NetId== netId).Include(r=>r.SubRole).ThenInclude(sub=>sub.Etiology).ToList();
            CacheService.Add(userSubRoles, cacheKey);
            return userSubRoles;
        }


        //public static IEnumerable<Prompt> GetPromptList(IPromptRepository promptRepository, int categoryId)
        //{
        //    var cacheKey = "PromptList" + categoryId;
        //    var promptList = CacheService.Get<IEnumerable<Prompt>>(cacheKey);
        //    if (promptList != null) return promptList;
        //    //var promptList = new IEnumerable<Prompt>();
        //    promptList =  promptRepository.Find(p => p.CategoryId == categoryId && p.Active == "Yes").OrderBy(p => p.Transcript).ToList();
        //    CacheService.Add(promptList, cacheKey);
        //    return promptList;
        //}
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
