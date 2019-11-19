using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IF
{
    public class ExportColumn
    {
        public ExportColumn(string FieldName, string HeadTitle)
        {
            this.FieldName = FieldName;
            this.HeadTitle = HeadTitle;
        }
        public string FieldName { get; private set; }
        public string HeadTitle { get; private set; }
    }
    public class ExportHelper
    {
        public static int GetStoreId(int StoreId)
        {
            switch (StoreId)
            {
                case 2:
                    return 50;
                case 4:
                    return 52;
                case 5:
                    return 57;
                case 7:
                    return 53;
                case 8:
                    return 56;
                case 9:
                    return 55;
                case 10:
                    return 54;
                case 11:
                    return 51;
                default:
                    return StoreId;
            }
        }

        private List<ExportColumn> column;
        public ExportHelper()
        {
            column = new List<ExportColumn>();
        }

        public void AddColumn(string FieldName, string HeadTitle)
        {
            column.Add(new ExportColumn(FieldName, HeadTitle));
        }

        public FileStreamResult ExportExcel<T>(List<T> entitys)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("sheet");
            IRow Title = null;
            IRow rows = null;
            Type entityType = typeof(T);

            for (int i = 0; i <= entitys.Count; i++)
            {
                if (i == 0)
                {
                    Title = sheet.CreateRow(0);
                    for (int k = 1; k <= column.Count; k++)
                    {
                        Title.CreateCell(0).SetCellValue("序号");
                        Title.CreateCell(k).SetCellValue(column[k - 1].HeadTitle);
                    }
                    continue;
                }
                else
                {
                    rows = sheet.CreateRow(i);
                    object entity = entitys[i - 1];
                    for (int j = 1; j <= column.Count; j++)
                    {
                        object[] entityValues = new object[column.Count];                      
                        entityValues[j - 1] = entityType.GetProperty(column[j - 1].FieldName).GetValue(entity);
                        rows.CreateCell(0).SetCellValue(i);
                        rows.CreateCell(j).SetCellValue(entityValues[j - 1].ToString());
                    }
                }
            }
            FileStreamResult fileStreamResult;
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                var memi = new FileExtensionContentTypeProvider().Mappings[".xlsx"];
                fileStreamResult = new FileStreamResult(ms, memi);
                ms.Close();
            }

            return fileStreamResult;
        }
    }

}
