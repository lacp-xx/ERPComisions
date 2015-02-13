using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using System.IO;
using ERPComisions.Models;
using ERPComisions.ViewModels;
using System.Data.OleDb;
using System.Data;
using Model;
//using ERPCommissionsModel;
using System.Threading.Tasks;
using OfficeOpenXml.Style;
using ERPCommissions.ImportExportUtil;


namespace ERPComisions.Controllers
{
    public class ImportReportController : Controller
    {
        // private ModelContext db = new ModelContext();
        private ErpModelContainer db = new ErpModelContainer();
        List<string> columnNames = new List<string>(new string[] { "Dealer", "Plan", "Spiff", "Ressidual", "Sim" });
        List<string> resumeColumnNames = new List<string>(new string[] { "Dealer", "Operator", "Spiff", "Ressidual" });

        // GET: ImportReport
        public ActionResult Index()
        {
            return View();
        }



        //void AddResumeWorksheet(ExcelPackage package, string sheetName)
        //{ 
        //    ExcelWorksheet ws = package.Workbook.Worksheets.Add(sheetName);
        //    CreateSheetHeader(ws,  sheetName, new List<string>(new string[] { "Dealer", "Plan", "Spiff", "Ressidual", "Sim" }));
        //    var query = from ar in db.ActivationReports
        //                from dfp in db.DefaultPlanCommissions
        //                from p in db.Plans
        //                where ar.OperatorId == 1 && ar.ActionDate.Month == month && ar.ActionDate.Year == year
        //                //                        join p in db.Plans on ar.PlanId equals p.Id
        //                //                      join dfp in db.DefaultPlanCommissions on p.Id equals dfp.PlanId
        //                group new
        //                {
        //                    ar.Operator.Name,
        //                    ar.ActionDate,
        //                    ar.Sim,
        //                    planName = p.Name,
        //                    planValue = p.Value,
        //                    customerNAme = ar.Customer.Name,
        //                    dfp.DealerCommissionValue
        //                } by ar.Customer.Name;

        //    int row = 2;
        //    int initGroupRow = 2;

        //    foreach (var group in query.ToList())
        //    {
        //        initGroupRow = row + 1;
        //        foreach (var item in group)
        //        {
        //            row++;
        //            ws.Cells[row, 1].Value = item.customerNAme;
        //            ws.Cells[row, 2].Value = item.planValue;
        //            ws.Cells[row, 3].Value = item.DealerCommissionValue;
        //            ws.Cells[row, 5].Value = item.Sim;
        //        }
        //        ws.Cells[initGroupRow, 1, row, 1].Merge = true;
        //        ws.Cells[initGroupRow, 1, row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

        //        row++;
        //        ws.Cells[row, 1].Value = "Total";
        //        ws.Cells[row, 1].Style.Font.Bold = true;
        //        ws.Cells[row, 3].Formula = string.Format("Sum({0})", new ExcelAddress(initGroupRow, 3, row - 1, 3).Address);
        //        ws.Cells[row, 3].Style.Font.Bold = true;

        //        //ws1.Cells[row, 1].Value = "Total";
        //        //ws1.Cells[row, 3].Value = 30;//.Formula = "Sum(" + ws.Cells[3, colIndex].Address + ":" + ws.Cells[rowIndex - 1, colIndex].Address + ")";
        //    }
        //}

        void CreateSheetHeader(ExcelWorksheet ws, string sheetName, List<string> columnsName)
        {
            ws.Name = sheetName; //Setting Sheet's name
            ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet


            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

            ws.Cells[1, 1].Value = sheetName; // Heading Name
            ws.Cells[1, 1, 1, columnsName.Count].Merge = true; //Merge columns start and end range
            ws.Cells[1, 1, 1, columnsName.Count].Style.Font.Bold = true; //Font should be bold
            ws.Cells[1, 1, 1, columnsName.Count].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Aligmnet is center

            int columnCount = 1;
            foreach (var column in columnsName)
            {
                ws.Cells[2, columnCount].Value = column; // Heading Name
                ws.Cells[2, columnCount].Style.Font.Bold = true; //Font should be bold
                ws.Cells[2, columnCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Aligmnet is center
                columnCount++;
            }
        }

        [HttpPost]
        public async Task<ActionResult> ImportResidualReport(PreviewExcelViewModel viewModel, HttpPostedFileBase file)
        {
            var sheetNumber = viewModel.excellSheetNumber;
            var carrierId = 1; // simple mobile = 1, net10 = 2
            var carrierName = "Simple Mobile";
            //PreviewExcelViewModel pe = new PreviewExcelViewModel();
            List<string> badSims = new List<string>();

            if (Request.Files["file"].ContentLength > 0)
            {
                string fileExtension = Path.GetExtension(Request.Files["file"].FileName);
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string filePath = Path.Combine(Server.MapPath("~/Content/Temp/"), System.Guid.NewGuid().ToString("N") + fileExtension);
                    if (System.IO.File.Exists(filePath))
                    {

                        System.IO.File.Delete(filePath);
                    }
                    Request.Files["file"].SaveAs(filePath);
                    ImportExcelViewModel importExcelViewModel;
                    FileInfo existingFile = new FileInfo(filePath);
                    using (ExcelPackage package = new ExcelPackage(existingFile))
                    {
                        //if (workbook != null)
                        importExcelViewModel = ParseTracfoneResidualWorksheet(package.Workbook.Worksheets[sheetNumber]);
                    }

                    foreach (var data in importExcelViewModel.ExcelData)
                    {
                        ResidualReport report = new ResidualReport();
                        report.CarrierId = carrierId;
                        report.CarrierName = carrierName;

                        // check if item exist by Sim, Esn, CardSmp or ByopActCardSmp
                        Item item = db.Items.FirstOrDefault(i => i.Serial == data.Sim);
                        if (item == null)
                            item = db.Items.FirstOrDefault(i => i.Serial == data.Esn);
                        if (item == null)
                            item = db.Items.FirstOrDefault(i => i.Serial == data.CardSmp);
                        if (item == null)
                            item = db.Items.FirstOrDefault(i => i.Serial == data.ByopActCardSmp);
                        if (item != null)
                        {
                            Customer customer = GetCustomerByCustNo(item.SelledTo);
                            report.Serial = item.Serial;
                            if (customer != null)
                            {
                                report.CustomerId = customer.Id;
                                report.CustNo = item.SelledTo;
                                report.CustomerName = customer.Name;
                            }
                            else
                            {
                                report.CustomerId = null;
                                report.CustNo = null;
                                report.CustomerName = null;
                            }
                        }
                        else
                        {
                            //save sim number, no aparece
                            badSims.Add(data.Sim);
                            report.CustomerId = null;
                            report.CustNo = null;
                            report.Serial = null;
                        }

                        report.Sim = data.Sim;
                        report.Esn = data.Esn;
                        report.CardSmp = data.CardSmp;
                        report.ByopActCardSmp = data.ByopActCardSmp;
                        report.ActionDate = DateTime.FromOADate(Double.Parse(data.ActiondDate));
                        report.Commission = Double.Parse(data.Commission);
                        report.PlanValue = Double.Parse(data.Plan);
                        //report.PlanId = GetPlanIdByValueAndOperator(data.Plan, report.CarrierId);

                        db.ResidualReports.Add(report);
                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                            if (typeof(System.Data.Entity.Validation.DbEntityValidationException).Equals(e.GetType()))
                            {
                                var ex = e as System.Data.Entity.Validation.DbEntityValidationException;
                            }
                            throw;
                        }
                    }
                    existingFile.Delete();
                }
            }
            return View("Index");
        }

        [HttpPost]
        public async Task<ActionResult> ImportSimpleReport(PreviewExcelViewModel viewModel, HttpPostedFileBase file)
        {
            var sheetNumber = viewModel.excellSheetNumber;
            var carrierId = 1; // simple mobile = 1, net10 = 2
            var carrierName = "Simple Mobile";
            //PreviewExcelViewModel pe = new PreviewExcelViewModel();
            List<string> badSims = new List<string>();

            if (Request.Files["file"].ContentLength > 0)
            {
                string fileExtension = Path.GetExtension(Request.Files["file"].FileName);
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string filePath = Path.Combine(Server.MapPath("~/Content/Temp/"), System.Guid.NewGuid().ToString("N") + fileExtension);
                    if (System.IO.File.Exists(filePath))
                    {

                        System.IO.File.Delete(filePath);
                    }
                    Request.Files["file"].SaveAs(filePath);
                    ImportExcelViewModel importExcelViewModel;
                    FileInfo existingFile = new FileInfo(filePath);
                    using (ExcelPackage package = new ExcelPackage(existingFile))
                    {
                        //if (workbook != null)
                        importExcelViewModel = ParseTracfoneWorksheet(package.Workbook.Worksheets[sheetNumber]);
                    }

                    foreach (var data in importExcelViewModel.ExcelData)
                    {
                        ActivationReport report = new ActivationReport();
                        report.CarrierId = carrierId;
                        report.CarrierName = carrierName;

                        // check if item exist by Sim, Esn, CardSmp or ByopActCardSmp
                        Item item = db.Items.FirstOrDefault(i => i.Serial == data.Sim);
                        if (item == null)
                            item = db.Items.FirstOrDefault(i => i.Serial == data.Esn);
                        if (item == null)
                            item = db.Items.FirstOrDefault(i => i.Serial == data.CardSmp);
                        if (item == null)
                            item = db.Items.FirstOrDefault(i => i.Serial == data.ByopActCardSmp);
                        if (item != null)
                        {
                            Customer customer = GetCustomerByCustNo(item.SelledTo);
                            report.Serial = item.Serial;
                            if (customer != null)
                            {
                                report.CustomerId = customer.Id;
                                report.CustNo = item.SelledTo;
                                report.CustomerName = customer.Name;
                            }
                            else {
                                report.CustomerId = null;
                                report.CustNo = null;
                                report.CustomerName = null;
                            }
                            //report.CommissionToCust = GetCommissionByCust(report.CustomerId, report.OperatorId);
                        }
                        else
                        {
                            //save sim number, no aparece
                            badSims.Add(data.Sim);
                            report.CustomerId = null;
                            report.CustNo = null;
                            report.Serial = null;
                        }


                        report.Sim = data.Sim;
                        report.Esn = data.Esn;
                        report.CardSmp = data.CardSmp;
                        report.ByopActCardSmp = data.ByopActCardSmp;
                        report.ActionDate = DateTime.FromOADate(Double.Parse(data.ActiondDate));
                        report.Commission = Double.Parse(data.Commission);
                        report.PlanValue = Double.Parse(data.Plan);
                        
                        //report.PlanId = GetPlanIdByValueAndOperator(data.Plan, report.CarrierId);


                        db.ActivationReports.Add(report);
                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception e)
                        {
                            if (typeof(System.Data.Entity.Validation.DbEntityValidationException).Equals(e.GetType()))
                            {
                                var ex = e as System.Data.Entity.Validation.DbEntityValidationException;
                            }
                            throw;
                        }

                        //return RedirectToAction("Index");
                    }
                    existingFile.Delete();


                    //ImportExcelViewModel importExcelViewModel = new ImportExcelViewModel();
                    //importExcelViewModel.ExcelData = new List<ActivationData>();
                    //ActivationData data = new ActivationData();


                    //PreviewExcelViewModel pe = new PreviewExcelViewModel();

                    //pe.Data = new List<List<string>>();

                    //for (int j = start.Row + 2; j <= end.Row; j++)
                    //{
                    //    List<string> column = new List<string>();

                    //    for (int i = start.Column; i <= end.Column; i++)
                    //    { // ... Cell by cell...

                    //        column.Add(worksheet.Cells[j, i].Text);
                    //    }
                    //    pe.Data.Add(column);

                    //    //data.Serial = worksheet.Cells[j, 1].Text;
                    //    //data.Plan = worksheet.Cells[j, 2].Text;
                    //    //data.ActivatedDate = Convert.ToDateTime(worksheet.Cells[j, 3].Text);

                    //    //importExcelViewModel.ExcelData.Add(data);
                    //}
                }
            }
            return View("Index");
        }



        public List<String> GetPlansByOperator(int operatorId)
        {

            List<String> result = new List<string>();
            return result;
        }

        public int GetPlanIdByValueAndOperator(string value, int carrierId)
        {
            Plan plan = null ;
            try {
                double doubleValue = Double.Parse(value);
                plan = db.Plans.FirstOrDefault(c => c.Value == doubleValue && c.CarrierId == carrierId);
            }catch(Exception e){}

            if (plan != null) return plan.Id;
            else
                return 0;

            //Article article = await db.Articles.FindAsync(id);
            //if (article == null)
            //{
            //    return HttpNotFound();
            //}
            //return View(article);

            //string planName = "";
        }

        public Customer GetCustomerByCustNo(string custNo)
        {
            Customer customer = db.Customers.FirstOrDefault(c => c.CustNo == custNo);
            if (customer != null)
                return customer;
            else
                return null;
        }

        public Customer GetCustomerBySerial(string serial)
        {

            Customer customer = db.Customers.FirstOrDefault(c => c.CustNo == serial);

            //Article article = await db.Articles.FindAsync(id);
            //if (article == null)
            //{
            //    return HttpNotFound();
            //}
            //return View(article);

            //string planName = "";
            return customer;
        }


        public List<String> ParseExcel(String filePath)
        {
            String html = "";
            List<string> result = new List<string>();
            FileInfo existingFile = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                // get the first worksheet in the workbook
                //                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                //              var start = worksheet.Dimension.Start;
                //            var end = worksheet.Dimension.End;

                var workbook = package.Workbook;
                if (workbook != null)
                {
                    for (int j = 1; j <= workbook.Worksheets.Count; j++)
                    {
                        html = "";
                        var worksheet = workbook.Worksheets[j];
                        if (worksheet.Dimension == null) { continue; }
                        html += "<style> table, th, td {border: 1px solid black;} th, td {padding: 8px;}</style>";
                        html += "<table style='border-collapse: collapse;font-family:arial; font-size:11px'><tbody>";
                        int rowCount = 0;
                        int maxColumnNumber = worksheet.Dimension.End.Column;
                        var convertedRecords = new List<List<string>>(worksheet.Dimension.End.Row);
                        var excelRows = worksheet.Cells.GroupBy(c => c.Start.Row).ToList();

                        html += String.Format("<tr>");
                        for (int k = 1; k <= maxColumnNumber; k++)
                            html += String.Format("<td>column {0}</td>", k);

                        html += String.Format("</tr>");

                        foreach (var r in excelRows)
                        {
                            rowCount++;
                            html += String.Format("<tr>");
                            var currentRecord = new List<string>(maxColumnNumber);
                            var cells = r.OrderBy(cell => cell.Start.Column).ToList();
                            Double rowHeight = worksheet.Row(rowCount).Height;
                            for (int i = 1; i <= maxColumnNumber; i++)
                            {
                                var currentCell = cells.Where(c => c.Start.Column == i).FirstOrDefault();

                                if (currentCell == null)
                                {
                                    html += String.Format("<td cellspacing>{0}</td>", String.Empty);
                                }
                                else
                                {
                                    int colSpan = 1;
                                    int rowSpan = 1;

                                    //check if this is the start of a merged cell
                                    ExcelAddress cellAddress = new ExcelAddress(currentCell.Address);

                                    var mCellsResult = (from c in worksheet.MergedCells
                                                        let addr = new ExcelAddress(c)
                                                        where cellAddress.Start.Row >= addr.Start.Row &&
                                                        cellAddress.End.Row <= addr.End.Row &&
                                                        cellAddress.Start.Column >= addr.Start.Column &&
                                                        cellAddress.End.Column <= addr.End.Column
                                                        select addr);

                                    if (mCellsResult.Count() > 0)
                                    {
                                        var mCells = mCellsResult.First();

                                        //if the cell and the merged cell do not share a common start address then skip this cell as it's already been covered by a previous item
                                        if (mCells.Start.Address != cellAddress.Start.Address)
                                            continue;

                                        if (mCells.Start.Column != mCells.End.Column)
                                        {
                                            colSpan += mCells.End.Column - mCells.Start.Column;
                                        }

                                        if (mCells.Start.Row != mCells.End.Row)
                                        {
                                            rowSpan += mCells.End.Row - mCells.Start.Row;
                                        }
                                    }
                                    //load up data
                                    html += String.Format("<td colspan={0} rowspan={1}>{2}</td>", colSpan, rowSpan, currentCell.Value);
                                }
                            }
                            html += String.Format("</tr>");
                            if (rowCount == 10) break;
                        };
                        html += "</tbody></table>";
                        result.Add(html);
                    }
                }
            }
            return result;
        }

        public ImportExcelViewModel ParseTracfoneWorksheet(ExcelWorksheet worksheet)
        {
            ImportExcelViewModel importExcelViewModel = new ImportExcelViewModel();
            importExcelViewModel.ExcelData = new List<ActivationData>();
            ActivationData data = new ActivationData();

            for (int j = worksheet.Dimension.Start.Row + 3; j <= worksheet.Dimension.End.Row-1; j++)
            {

                //for (int i = worksheet.Dimension.Start.Column; i <= worksheet.Dimension.End.Column; i++)
                //{ // ... Cell by cell...
                if (worksheet.Cells[j, 7].Value != null || worksheet.Cells[j, 8].Value != null || worksheet.Cells[j, 9].Value != null)
                {
                    data = new ActivationData();
                    data.Sim = worksheet.Cells[j, 7].Value != null ? worksheet.Cells[j, 7].Value.ToString() : "0";
                    data.Esn = worksheet.Cells[j, 8].Value != null ? worksheet.Cells[j, 8].Value.ToString() : "0";
                    data.CardSmp = worksheet.Cells[j, 9].Value != null ? worksheet.Cells[j, 9].Value.ToString() : "0";
                    data.ByopActCardSmp = worksheet.Cells[j, 10].Value != null ? worksheet.Cells[j, 10].Value.ToString() : "0";
                    data.ActiondDate = worksheet.Cells[j, 11].Value != null ? worksheet.Cells[j, 11].Value.ToString() : "0";
                    data.Commission = worksheet.Cells[j, 14].Value != null ? worksheet.Cells[j, 14].Value.ToString() : "0";
                    data.Plan = worksheet.Cells[j, 13].Value != null ? worksheet.Cells[j, 13].Value.ToString() : "0";

                    importExcelViewModel.ExcelData.Add(data);
                }
            }

            //if (worksheet.Dimension != null)
            //{
            //    int rowCount = 0;
            //    int maxColumnNumber = worksheet.Dimension.End.Column;
            //    var convertedRecords = new List<List<string>>(worksheet.Dimension.End.Row);
            //    var excelRows = worksheet.Cells.GroupBy(c => c.Start.Row).ToList();

            //    foreach (var r in excelRows)
            //    {
            //        rowCount++;
            //        var currentRecord = new List<string>(maxColumnNumber);
            //        var cells = r.OrderBy(cell => cell.Start.Column).ToList();
            //        Double rowHeight = worksheet.Row(rowCount).Height;
            //        for (int i = 1; i <= maxColumnNumber; i++)
            //        {
            //            var currentCell = cells.Where(c => c.Start.Column == i).FirstOrDefault();

            //            if (currentCell == null)
            //            {
            //            }
            //            else
            //            {
            //                //load up data
            //                //                            html += String.Format("<td colspan={0} rowspan={1}>{2}</td>", colSpan, rowSpan, currentCell.Value);
            //            }
            //        }
            //    };
            //}
            return importExcelViewModel;
        }

        public ImportExcelViewModel ParseTracfoneResidualWorksheet(ExcelWorksheet worksheet)
        {
            ImportExcelViewModel importExcelViewModel = new ImportExcelViewModel();
            importExcelViewModel.ExcelData = new List<ActivationData>();
            ActivationData data = new ActivationData();

            for (int j = worksheet.Dimension.Start.Row + 3; j <= worksheet.Dimension.End.Row - 1; j++)
            {

                //for (int i = worksheet.Dimension.Start.Column; i <= worksheet.Dimension.End.Column; i++)
                //{ // ... Cell by cell...
                if (worksheet.Cells[j, 7].Value != null || worksheet.Cells[j, 8].Value != null || worksheet.Cells[j, 9].Value != null)
                {
                    data = new ActivationData();
                    data.Sim = worksheet.Cells[j, 7].Value != null ? worksheet.Cells[j, 7].Value.ToString() : "0";
                    data.Esn = worksheet.Cells[j, 8].Value != null ? worksheet.Cells[j, 8].Value.ToString() : "0";
                    data.CardSmp = worksheet.Cells[j, 9].Value != null ? worksheet.Cells[j, 9].Value.ToString() : "0";
                    data.ByopActCardSmp = worksheet.Cells[j, 10].Value != null ? worksheet.Cells[j, 10].Value.ToString() : "0";
                    data.ActiondDate = worksheet.Cells[j, 2].Value != null ? worksheet.Cells[j, 2].Value.ToString() : "0";
                    data.Commission = worksheet.Cells[j, 13].Value != null ? worksheet.Cells[j, 13].Value.ToString() : "0";
                    data.Plan = worksheet.Cells[j, 12].Value != null ? worksheet.Cells[j, 12].Value.ToString() : "0";

                    importExcelViewModel.ExcelData.Add(data);
                }
            }

            return importExcelViewModel;
        }


        public List<String> ParseRedPocketWorksheet(String filePath)
        {
            String html = "";
            List<string> result = new List<string>();
            FileInfo existingFile = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                // get the first worksheet in the workbook
                //                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                //              var start = worksheet.Dimension.Start;
                //            var end = worksheet.Dimension.End;

                var workbook = package.Workbook;
                if (workbook != null)
                {
                    for (int j = 1; j <= workbook.Worksheets.Count; j++)
                    {
                        html = "";
                        var worksheet = workbook.Worksheets[j];
                        if (worksheet.Dimension == null) { continue; }
                        html += "<style> table, th, td {border: 1px solid black;} th, td {padding: 8px;}</style>";
                        html += "<table style='border-collapse: collapse;font-family:arial; font-size:11px'><tbody>";
                        int rowCount = 0;
                        int maxColumnNumber = worksheet.Dimension.End.Column;
                        var convertedRecords = new List<List<string>>(worksheet.Dimension.End.Row);
                        var excelRows = worksheet.Cells.GroupBy(c => c.Start.Row).ToList();

                        html += String.Format("<tr>");
                        for (int k = 1; k <= maxColumnNumber; k++)
                            html += String.Format("<td>column {0}</td>", k);

                        html += String.Format("</tr>");

                        foreach (var r in excelRows)
                        {
                            rowCount++;
                            html += String.Format("<tr>");
                            var currentRecord = new List<string>(maxColumnNumber);
                            var cells = r.OrderBy(cell => cell.Start.Column).ToList();
                            Double rowHeight = worksheet.Row(rowCount).Height;
                            for (int i = 1; i <= maxColumnNumber; i++)
                            {
                                var currentCell = cells.Where(c => c.Start.Column == i).FirstOrDefault();

                                if (currentCell == null)
                                {
                                    html += String.Format("<td cellspacing>{0}</td>", String.Empty);
                                }
                                else
                                {
                                    int colSpan = 1;
                                    int rowSpan = 1;

                                    //check if this is the start of a merged cell
                                    ExcelAddress cellAddress = new ExcelAddress(currentCell.Address);

                                    var mCellsResult = (from c in worksheet.MergedCells
                                                        let addr = new ExcelAddress(c)
                                                        where cellAddress.Start.Row >= addr.Start.Row &&
                                                        cellAddress.End.Row <= addr.End.Row &&
                                                        cellAddress.Start.Column >= addr.Start.Column &&
                                                        cellAddress.End.Column <= addr.End.Column
                                                        select addr);

                                    if (mCellsResult.Count() > 0)
                                    {
                                        var mCells = mCellsResult.First();

                                        //if the cell and the merged cell do not share a common start address then skip this cell as it's already been covered by a previous item
                                        if (mCells.Start.Address != cellAddress.Start.Address)
                                            continue;

                                        if (mCells.Start.Column != mCells.End.Column)
                                        {
                                            colSpan += mCells.End.Column - mCells.Start.Column;
                                        }

                                        if (mCells.Start.Row != mCells.End.Row)
                                        {
                                            rowSpan += mCells.End.Row - mCells.Start.Row;
                                        }
                                    }
                                    //load up data
                                    html += String.Format("<td colspan={0} rowspan={1}>{2}</td>", colSpan, rowSpan, currentCell.Value);
                                }
                            }
                            html += String.Format("</tr>");
                            if (rowCount == 10) break;
                        };
                        html += "</tbody></table>";
                        result.Add(html);
                    }
                }
            }
            return result;
        }
        public String ParseH2OWorksheet(ExcelWorksheet worksheet)
        {
            if (worksheet.Dimension != null)
            {
                int rowCount = 0;
                int maxColumnNumber = worksheet.Dimension.End.Column;
                var convertedRecords = new List<List<string>>(worksheet.Dimension.End.Row);
                var excelRows = worksheet.Cells.GroupBy(c => c.Start.Row).ToList();

                foreach (var r in excelRows)
                {
                    rowCount++;
                    var currentRecord = new List<string>(maxColumnNumber);
                    var cells = r.OrderBy(cell => cell.Start.Column).ToList();
                    Double rowHeight = worksheet.Row(rowCount).Height;
                    for (int i = 1; i <= maxColumnNumber; i++)
                    {
                        var currentCell = cells.Where(c => c.Start.Column == i).FirstOrDefault();

                        if (currentCell == null)
                        {
                        }
                        else
                        {
                            int colSpan = 1;
                            int rowSpan = 1;

                            //check if this is the start of a merged cell
                            ExcelAddress cellAddress = new ExcelAddress(currentCell.Address);

                            var mCellsResult = (from c in worksheet.MergedCells
                                                let addr = new ExcelAddress(c)
                                                where cellAddress.Start.Row >= addr.Start.Row &&
                                                cellAddress.End.Row <= addr.End.Row &&
                                                cellAddress.Start.Column >= addr.Start.Column &&
                                                cellAddress.End.Column <= addr.End.Column
                                                select addr);

                            if (mCellsResult.Count() > 0)
                            {
                                var mCells = mCellsResult.First();

                                //if the cell and the merged cell do not share a common start address then skip this cell as it's already been covered by a previous item
                                if (mCells.Start.Address != cellAddress.Start.Address)
                                    continue;

                                if (mCells.Start.Column != mCells.End.Column)
                                {
                                    colSpan += mCells.End.Column - mCells.Start.Column;
                                }

                                if (mCells.Start.Row != mCells.End.Row)
                                {
                                    rowSpan += mCells.End.Row - mCells.Start.Row;
                                }
                            }
                            //load up data
                            //                            html += String.Format("<td colspan={0} rowspan={1}>{2}</td>", colSpan, rowSpan, currentCell.Value);
                        }
                    }
                };
            }
            return "";

        }
    }
}