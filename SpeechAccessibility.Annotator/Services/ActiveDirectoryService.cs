using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using Microsoft.IdentityModel.Protocols;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Core.Models;


namespace SpeechAccessibility.Annotator.Services
{
    public class ActiveDirectoryService
    {
        public static bool IsUserGroupMember(string netId, string groupName, string domain, string env)
        {
            if (env != "Production")
            {
                if (groupName != "Beckman WebApps Admins" && groupName != "Beckman ITS FT")
                    groupName += " DEV";
            }
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                UserPrincipal currentUser = UserPrincipal.FindByIdentity(context, netId);
                if (currentUser == null) return false;
                GroupPrincipal group = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
                if (group == null) return false;
                if (currentUser.IsMemberOf(group))
                {
                    return true;
                }

                var nestedGroups = group.GetMembers();

                //check nested groups
                foreach (var nestedGroup in nestedGroups.OfType<GroupPrincipal>())
                {
                    return IsUserGroupMember(netId, nestedGroup.SamAccountName, domain, env);
                }
            }

            return false;

        }
        public static IQueryable<ADMemberViewModel> GetAllMembers(List<Role> roles, string searchValue, string domain, string env) {
            
            List<ADMemberViewModel> users = new List<ADMemberViewModel>();
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                foreach (var role in roles)
                {
                    if (env != "Production")
                    {
                        if (role.ADGroupName != "Beckman WebApps Admins" && role.ADGroupName != "Beckman ITS FT")
                            role.ADGroupName += " DEV";
                    }
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(context, IdentityType.Name, role.ADGroupName);
                    if (group != null)
                    {
                        foreach (Principal member in group.Members)
                        {
                            //check for nested group
                            if (member.StructuralObjectClass == "group")
                            {
                                GroupPrincipal nestedGroup = GroupPrincipal.FindByIdentity(context, IdentityType.Name, member.SamAccountName);
                                GetMembersFromNestedGroup(context,searchValue, nestedGroup, role, users);
                            }
                            else
                            {
                                var temp = new ADMemberViewModel
                                {
                                    NetId = member.SamAccountName,
                                    RoleId = role.Id,
                                    ADGroupName = role.ADGroupName,
                                    RoleName = role.Name
                                };

                                if (member.DisplayName != "")
                                {
                                    string[] names = member.DisplayName.Split(',');
                                    if (names.Length > 1)
                                    {
                                        temp.LastName = names[0];
                                        temp.FirstName = names[1];
                                    }
                                }

                                if (temp.NetId.ToLower().Contains(searchValue.ToLower()) ||
                                    temp.LastName.ToLower().Contains(searchValue.ToLower()) ||
                                    temp.FirstName.ToLower().Contains(searchValue.ToLower()) ||
                                    temp.RoleName.ToLower().Contains(searchValue.ToLower()) )
                                {
                                    users.Add(temp);
                                }
                            }
                        }
                    }
                }
            }

            return users.AsQueryable();
        }

        private static void GetMembersFromNestedGroup(PrincipalContext context, string searchValue, GroupPrincipal group, Role role, List<ADMemberViewModel> users)
        {
            var nestedGroups = group.GetMembers();
            ////check nested groups
            foreach (var gr in nestedGroups.OfType<GroupPrincipal>())
            {
                foreach (Principal subMember in gr.Members)
                {
                    if (subMember.StructuralObjectClass == "group")
                    {
                        GroupPrincipal nestedGroup = GroupPrincipal.FindByIdentity(context, IdentityType.Name, subMember.SamAccountName);
                        GetMembersFromNestedGroup(context,searchValue, nestedGroup, role, users);
                    }
                    else
                    {
                        var temp = new ADMemberViewModel
                        {
                            NetId = subMember.SamAccountName,
                            RoleId = role.Id,
                            ADGroupName = role.ADGroupName,
                            RoleName = role.Name
                        };
                        if (subMember.DisplayName != "")
                        {
                            string[] names = subMember.DisplayName.Split(',');
                            if (names.Length > 1)
                            {
                                temp.LastName = names[0];
                                temp.FirstName = names[1];
                            }
                        }

                        if (temp.NetId.ToLower().Contains(searchValue.ToLower()) ||
                            temp.LastName.ToLower().Contains(searchValue.ToLower()) ||
                            temp.FirstName.ToLower().Contains(searchValue.ToLower()) ||
                            temp.RoleName.ToLower().Contains(searchValue.ToLower()))
                        {
                            users.Add(temp);
                        }
                    }
                    
                }
            }
        }

        public static ADMemberViewModel GetUserInfoFromAD(string currentUserNetId, string domain)
        {
            if (string.IsNullOrEmpty(currentUserNetId))
                return null;
            using (var context = new PrincipalContext(ContextType.Domain,domain))
            {
                var principal = UserPrincipal.FindByIdentity(context, currentUserNetId);
                var user = new ADMemberViewModel();
                if (principal != null)
                {
                    user.LastName = principal.Surname;
                    user.FirstName = principal.GivenName;
                    user.NetId = currentUserNetId;
                }
                else
                {
                    user.LastName = "";
                    user.FirstName = "";
                    user.NetId = "";
                }
                return user;
            }
        }

        public static string RemoveMemberFromADGroup(string netId, string groupName, string domain, string container, string env)
        {
            var error = "";
            if (env != "Production")
            {
                if (groupName != "Beckman WebApps Admins" && groupName != "Beckman ITS FT")
                    groupName += " DEV";
            }
            using (var context = new PrincipalContext(ContextType.Domain, domain, container))
            {
                UserPrincipal member = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, netId);

                if (member != null)
                {
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        GroupPrincipal group = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
                        if (group != null)
                        {
                            if (member.IsMemberOf(group))
                            {
                                try
                                {
                                    group.Members.Remove(context, IdentityType.UserPrincipalName, member.UserPrincipalName);
                                    group.Save();
                                    group.Dispose();
                                }
                                catch (Exception e)
                                {
                                    error = e.Message;
                                }
                            }
                        }
                        else
                        {
                            error = error + "Group " + groupName + " not found from AD. ";
                        }
                    }

                }
                else
                {
                    error = error + "Member " + netId + " not found from AD. ";
                }

                context.Dispose();
            }
            return error;
        }
        public static string AddMemberToADGroup(string netId, string groupName, string domain, string container, string env)
        {
            if (env != "Production")
            {
                if (groupName != "Beckman WebApps Admins" && groupName != "Beckman ITS FT")
                    groupName += " DEV";
            }
            if (!string.IsNullOrEmpty(groupName))
            {
                var error = "";
                using (var context = new PrincipalContext(ContextType.Domain, domain, container))
                {
                    UserPrincipal member = UserPrincipal.FindByIdentity(context, netId);
                    if (member != null)
                    {
                        GroupPrincipal group = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
                        if (group != null)
                        {
                            if (!member.IsMemberOf(group))
                            {
                                try
                                {
                                    group.Members.Add(context, IdentityType.UserPrincipalName, member.UserPrincipalName);
                                    group.Save();
                                    group.Dispose();

                                }
                                catch (Exception e)
                                {
                                    error = e.Message;
                                }
                            }
                        }
                        else
                        {
                            error = error + "Group " + groupName + " not found from AD. ";
                        }
                    }
                    else
                    {
                        error = error + "Member " + netId + " not found from AD. ";
                    }
                    context.Dispose();
                }
                return error;
            }

            return "";

        }

        public static string GetMemberADGroupName(string netId, List<Role> roles, string domain, string env)
        {
            foreach (var role in roles.Where(role => IsUserGroupMember(netId, role.ADGroupName, domain, env) == true))
            {
                return role.ADGroupName;
            }

            return "";
        }

    }
}
