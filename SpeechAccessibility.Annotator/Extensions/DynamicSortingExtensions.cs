using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SpeechAccessibility.Annotator.Models;
using SpeechAccessibility.Core.Models;

namespace SpeechAccessibility.Annotator.Extensions
{
    //public static class DynamicSortingExtensions<T> where T : BaseEntity
    public static class DynamicSortingExtensions<T> 
    {
        public static IQueryable<T> SetOrderByDynamic(IQueryable<T> list, string orderByColumn)
        {
            list = list.OrderBy(orderByColumn);
            return list;
        }

        public static IQueryable<T> SetOrderByDynamic(IQueryable<T> list, IFormCollection collection)
        {
            //var columnIndex = requestFormData["order[0][column]"].ToString();
            //var sortDirection = requestFormData["order[0][dir]"].ToString();

            var firstSortColumn = collection["columns[" + collection["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var firstSortColumnDir = collection["order[0][dir]"].FirstOrDefault();

            var firstColumnOrderByString = "";
            firstColumnOrderByString += firstSortColumn + " " + firstSortColumnDir.ToUpper();
            list = SetOrderByDynamic(list, firstColumnOrderByString);

            if (!string.IsNullOrEmpty(collection["order[1][column]"]))
            {
                var secondSortColumn = collection["columns[" + collection["order[1][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var secondSortColumnDir = collection["order[1][dir]"].FirstOrDefault();

                var secondColumnOrderByString = "";
                secondColumnOrderByString += secondSortColumn + " " + secondSortColumnDir.ToUpper();
                list = SetOrderByDynamic(list, firstColumnOrderByString + ", " + secondColumnOrderByString);
            }
            else
            {
                list = SetOrderByDynamic(list, firstColumnOrderByString);
            }
            return list;
        }

        public static List<T> SetOrderByDynamic(List<T> list, IFormCollection collection)
        {
            var firstSortColumn = collection["columns[" + collection["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var firstSortColumnDir = collection["order[0][dir]"].FirstOrDefault();

            var firstColumnOrderByString = "";
            firstColumnOrderByString += firstSortColumn + " " + firstSortColumnDir.ToUpper();
            //list = SetOrderByDynamic(list, firstColumnOrderByString);
            list = (List<T>)list.OrderBy(firstColumnOrderByString);

            if (!string.IsNullOrEmpty(collection["order[1][column]"]))
            {
                var secondSortColumn = collection["columns[" + collection["order[1][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var secondSortColumnDir = collection["order[1][dir]"].FirstOrDefault();

                var secondColumnOrderByString = "";
                secondColumnOrderByString += secondSortColumn + " " + secondSortColumnDir.ToUpper();
                //list = SetOrderByDynamic(list, firstColumnOrderByString + ", " + secondColumnOrderByString);
                list = (List<T>)list.OrderBy(firstColumnOrderByString + ", " + secondColumnOrderByString);
            }
            else
            {
                //list = SetOrderByDynamic(list, firstColumnOrderByString);
                list = (List<T>)list.OrderBy(firstColumnOrderByString);
            }
            return list;
        }
    }
}
