﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SpeechAccessibility.Core.Interfaces;

namespace SpeechAccessibility.Annotator.Extensions
{
    public class UtilsExtension
    {
        public static bool IsMatchedRole(IUserSubRoleRepository userSubRoleRepository,int etiologyId, string netId)
        {
            //var hasSubRole = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.OtherPhone)?.Value;
            //if (hasSubRole == "Yes")
            //{
                var subRoleList = userSubRoleRepository.Find(s => s.User.NetId == netId).Include(s=>s.SubRole.Etiology);
                var matchRole = subRoleList.Any(r => r.SubRole.Etiology.Id == etiologyId);
                if (!matchRole)
                {
                    return false;

                }
            //}
            return true;
        }

        public static string GetEtiologyName(IContributorRepository contributorRepository, Guid contributorId)
        {
            var contributor = contributorRepository.Find(c => c.Id == contributorId).Include(c => c.Etiology).FirstOrDefault();
            return contributor != null ? contributor.Etiology.Name : "";
        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

    }
}