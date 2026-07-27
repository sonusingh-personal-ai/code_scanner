using BusinessLogicLayer;
using Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeScanner.Controllers
{
    public class GenerateExcelController : Controller
    {
        // GET: GenerateExcel
        public ActionResult Index(/*List<int> ids*/)
        {
            var objENResponse = new enResponse();
            var objBLResponse = new blResponse(objENResponse);

            List<enResponse> listOfResponses = new List<enResponse>();
            try
            {
                listOfResponses = objBLResponse.ReadAllAndAggregate(typeof(enResponseSummary))/*.Where(x => ids.Contains(x.Id)).ToList()*/;
            }
            catch (Exception ex)
            {
                throw;
            }

            Microsoft.Office.Interop.Excel.Application excel;
            Microsoft.Office.Interop.Excel.Workbook worKbooK;
            Microsoft.Office.Interop.Excel.Worksheet worksheet;
            Microsoft.Office.Interop.Excel.Range celLrangE;

            excel = new Microsoft.Office.Interop.Excel.Application();
            excel.Visible = false;
            excel.DisplayAlerts = false;
            worKbooK = excel.Workbooks.Add(Type.Missing);


            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worKbooK.ActiveSheet;
            worksheet.Name = listOfResponses[0].Barcode;

            worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 8]].Merge();
            worksheet.Cells[1, 1] = "Testing ZIG Report";
            worksheet.Cells.Font.Size = 12;
            int rowcount = 2;

            foreach (DataRow datarow in ExportToExcel(listOfResponses).Rows)
            {
                rowcount += 1;
                for (int i = 1; i <= ExportToExcel(listOfResponses).Columns.Count; i++)
                {
                    var str = datarow[i - 1].ToString();
                    if (str == "VISUAL" || str == "DATE & TIME" || str == "PRO LINE" || str == "TESTED BY" || str == "LINE INCHARGE" || str == "ZIG NO" || str == "MODEL" || str == "SYS SR NO" || str == "CON PROG NO" || str == "DIS PROG NO")
                    {
                        worksheet.Cells.Font.Color = System.Drawing.Color.Green;
                        worksheet.Cells.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                        worksheet.Range[worksheet.Cells[rowcount, 2], worksheet.Cells[rowcount, 4]].Merge();
                        worksheet.Cells[rowcount, i] = datarow[i - 1].ToString();
                    }
                    else
                    {
                        worksheet.Cells[rowcount, i] = datarow[i - 1].ToString();
                    }
                }
            }

            celLrangE = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[rowcount, ExportToExcel(listOfResponses).Columns.Count]];
            celLrangE.EntireColumn.AutoFit();
            Microsoft.Office.Interop.Excel.Borders border = celLrangE.Borders;
            border.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            border.Weight = 2d;

            celLrangE = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[2, ExportToExcel(listOfResponses).Columns.Count]];

            worKbooK.SaveAs("textBox1.xlsx");
            worKbooK.Close();
            excel.Quit();
            return View();
        }

        public System.Data.DataTable ExportToExcel(List<enResponse> listOfResponses)
        {
            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("A", typeof(string));

            var i = 0;
            foreach (var item in listOfResponses)
            {
                table.Columns.Add((i++).ToString(), typeof(string));
                table.Columns.Add((i++).ToString(), typeof(string));
                table.Columns.Add((i++).ToString(), typeof(string));

                if (i % 3 == 0 && i > 4)
                {
                    table.Rows[0].SetField(i - 2, item.CurrentDate);
                    table.Rows[1].SetField(i - 2, item.VisualBy);
                    table.Rows[2].SetField(i - 2, item.ProductionLine);
                    table.Rows[3].SetField(i - 2, item.TestedBy);
                    table.Rows[4].SetField(i - 2, item.LineInCharge);
                    table.Rows[5].SetField(i - 2, item.TestingJig);
                    table.Rows[6].SetField(i - 2, item.Model);
                    table.Rows[7].SetField(i - 2, item.Barcode);
                    table.Rows[8].SetField(i - 2, item.ConProgNo);
                    table.Rows[9].SetField(i - 2, item.DisProgNo);
                    if (item.listOfResponseSummary.Count > 0)
                    {
                        table.Rows[10].SetField(i - 2, "DISPLAY");
                        table.Rows[10].SetField(i - 1, "ACTUAL");
                        table.Rows[10].SetField(i, "STATUS");
                    }
                    
                    var j = 11;
                    foreach (var item2 in item.listOfResponseSummary)
                    {
                        table.Rows[j].SetField(i - 2, item2.Dispaly);
                        table.Rows[j].SetField(i - 1, item2.Actual);
                        table.Rows[j].SetField(i, item2.Status);
                        j++;
                    }
                }
                else
                {
                    table.Rows.Add("DATE & TIME", item.CurrentDate);
                    table.Rows.Add("VISUAL", item.VisualBy);
                    table.Rows.Add("PRO LINE", item.ProductionLine);
                    table.Rows.Add("TESTED BY", item.TestedBy);
                    table.Rows.Add("LINE INCHARGE", item.LineInCharge);
                    table.Rows.Add("ZIG NO", item.TestingJig);
                    table.Rows.Add("MODEL", item.Model);
                    table.Rows.Add("SYS SR NO", item.Barcode);
                    table.Rows.Add("CON PROG NO.", item.ConProgNo);
                    table.Rows.Add("DIS PROG NO.", item.DisProgNo);

                    if (item.listOfResponseSummary.Count > 0)
                    {
                        table.Rows.Add("PARAMETERS", "DISPLAY", "ACTUAL", "STATUS");
                    }

                    foreach (var item2 in item.listOfResponseSummary)
                    {
                        table.Rows.Add(item2.Parameters, item2.Dispaly, item2.Actual, item2.Status);
                    }
                }
            }
            return table;
        }
    }
}