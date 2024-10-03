using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using SpeechAccessibility.Core.Interfaces;

namespace SpeechAccessibility.Annotator.Extensions
{
    public class UtilsExtension
    {
        public static bool IsMatchedRole(IUserSubRoleRepository userSubRoleRepository, int etiologyId, string netId)
        {
            var subRoleList = userSubRoleRepository.Find(s => s.User.NetId == netId);
            var matchRole = subRoleList.Any(r => r.SubRole.EtiologyId == etiologyId);
            if (!matchRole)
            {
                return false;

            }

            return true;
        }

        public static string GetEtiologyName(IContributorViewRepository contributorViewRepository, Guid contributorId)
        {
            return contributorViewRepository.Find(e => e.Id == contributorId).FirstOrDefault()?.EtiologyName;

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
    public class DataTableToExcel
    {
        public void DataToExcel(DataTable dt, MemoryStream ms)
        {
            using (dt)
            {
                IWorkbook workbook = new HSSFWorkbook(); //Create an excel Workbook
                var sheet = workbook.CreateSheet(); //Create a work table in the table
                var headerRow = sheet.CreateRow(0); //To add a row in the table
                foreach (DataColumn column in dt.Columns)
                    headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption);
                var rowIndex = 1;
                var doubleFormat = workbook.CreateDataFormat().GetFormat("#,##0.00"); //"#,##0.###");
                foreach (DataRow row in dt.Rows)
                {
                    var dataRow = sheet.CreateRow(rowIndex);
                    foreach (DataColumn column in dt.Columns)
                    {
                        ICell newCell;
                        switch (column.DataType.Name)
                        {
                            case "Decimal":
                            case "Double":
                                newCell = dataRow.CreateCell(column.Ordinal);
                                newCell.SetCellType(CellType.Numeric);
                                if (!string.IsNullOrEmpty(row[column].ToString()))
                                    newCell.SetCellValue(Convert.ToDouble(row[column]));
                                else
                                    newCell.SetCellValue(Convert.ToDouble(0));
                                newCell.CellStyle.DataFormat = doubleFormat;
                                break;
                            case "DateTime":
                                newCell = dataRow.CreateCell(column.Ordinal);
                                newCell.SetCellValue(!string.IsNullOrEmpty(row[column].ToString())
                                    ? $"{Convert.ToDateTime(row[column]): MM/dd/yyyy}"
                                    : "");
                                break;
                            default:
                                dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                                break;
                        }
                    }

                    rowIndex++;
                }

                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
            }
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }


    }
}