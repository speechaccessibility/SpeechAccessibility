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
       
        public static List<EtiologyView> GetEtiologyList(IEtiologyViewRepository etiologyRepository)
        {
            //remove cache since users will not get updated eitologies if we change their roles
            //var cacheKey = "Etiologies";
            //var etiologies = CacheService.Get<List<EtiologyView>>(cacheKey);
            //if (etiologies != null)
            //{
            //    return etiologies;
            //}
            var etiologies = etiologyRepository.Find(e => e.Active == "Yes").OrderBy(r => r.DisplayOrder).ToList();

            //CacheService.Add(etiologies, cacheKey);
            return etiologies;
        }

        public static List<UserSubRole> GetSubRoles(IUserSubRoleRepository userSubRoleRepository, string netId)
        {
            //remove cache
            //var cacheKey = "UserSubRole";
            //var userSubRoles = CacheService.Get<List<UserSubRole>>(cacheKey);
            //if (userSubRoles != null)
            //{
            //    return userSubRoles;
            //}
            
            var userSubRoles = userSubRoleRepository.Find(r => r.User.NetId == netId).Include(r => r.SubRole).ToList();

            //CacheService.Add(userSubRoles, cacheKey);
            return userSubRoles;
        }

        public static List<RegisterLink> GetRegisterLinks(IRegisterLinkRepository registerLinkRepository)
        {
            
            var cacheKey = "RegisterLinks";
            var registerLinks = CacheService.Get<List<RegisterLink>>(cacheKey);
            if (registerLinks != null)
            {
                return registerLinks;
            }
            registerLinks = registerLinkRepository.GetAll().ToList();
            CacheService.Add(registerLinks, cacheKey);
            return registerLinks;
        }

    }
}
