using Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERPComisions.Controllers
{
    public class ResumeData
    {
        public string DealerId { get; set; }
        public string DealerName { get; set; }
        public string Carrier { get; set; }
        public int CarrierId { get; set; }
        public double Commission { get; set; }
        public double DealerCommission { get; set; }

        public ResumeData()
        {
        }
    }

    public class ResumeDealerData
    {
        public string Carrier { get; set; }
        public double Commission { get; set; }

        public ResumeDealerData()
        {
        }
    }

    public class ReupData
    {
        public int Carrier { get; set; }
        public string CustNo { get; set; }

        public ReupData(int carrier, string custno)
        {
            Carrier = carrier;
            CustNo = custno;
        }
    }

    public class GenerateReportController : Controller
    {
        //85 189 223 Azul oscuro logo
        //97 198 233 Azul claro logo

        System.Drawing.Color headerFillColor = System.Drawing.Color.FromArgb(97, 198, 233);
        System.Drawing.Color defaultTextColor = System.Drawing.Color.Black;
        System.Drawing.Color ceroValueTextColor = System.Drawing.Color.Red;
        System.Drawing.Color summaryTextColor = System.Drawing.Color.FromArgb(85, 189, 223);
        System.Drawing.Color columnMasterFillColor = System.Drawing.Color.FromArgb(217, 217, 217);
        System.Drawing.Color headerTextColor = System.Drawing.Color.White;

        private ErpModelContainer db = new ErpModelContainer();

        // GET: GenerateReport
        public ActionResult Index()
        {
            return View();
        }

        // GET: Report
        public ActionResult CreateReport()
        {
            //CreateGeneralSpiffReport();
            CreateReports(11, 2014);
            return View();
        }

        private void CreateReports(int month, int year)
        {
            //var customers = db.Customers.Select(c => new { c.CustNo, MinValue = c.PlanCommissions.Select(p => p.MinValue), MaxValue = c.PlanCommissions.Select(p => p.MaxValue), DealerCommissionValue = c.PlanCommissions.Select(p => p.DealerCommissionValue), CommissionType = c.PlanCommissions.Select(p => p.CommissionType) });

            var customers = from c in db.Customers
                            from e in c.PlanCommissions
                            select new { c.CustNo, e.MinValue, e.MaxValue, e.DealerCommissionValue, e.CommissionType, e.CarrierId };

            var activationsSpiff = customers.Join(db.ActivationReports, c => c.CustNo, a => a.CustNo, (c, a) => new { a, c }).Where(o => o.a.PlanValue >= o.c.MinValue && o.a.PlanValue <= o.c.MaxValue && o.c.CarrierId == o.a.CarrierId && o.c.CommissionType == Model.CommissionType.Spiff && o.a.ActionDate.Month == month && o.a.ActionDate.Year == year);
            var activationsSpiffList = activationsSpiff.OrderBy(a => a.a.CustNo).ToList();
            var customersListSpiff = activationsSpiff.Select(a => new { a.a.CustNo, a.a.CustomerName }).Distinct().ToList();
            var carriersListSpiff = activationsSpiff.Select(a => new { a.a.CarrierId }).Distinct().Join(db.Carriers, a => a.CarrierId, c => c.Id, (a, c) => new { c.Id, c.Name }).ToList();

            var activationsResidual = customers.Join(db.ResidualReports, c => c.CustNo, a => a.CustNo, (c, a) => new { a, c }).Where(o => o.a.PlanValue >= o.c.MinValue && o.a.PlanValue <= o.c.MaxValue && o.c.CarrierId == o.a.CarrierId && o.c.CommissionType == Model.CommissionType.Residual);
            var activationsResidualList = activationsResidual.OrderBy(a => a.a.CustNo).ToList();
            var customersListResidual = activationsResidual.Select(a => new { a.a.CustNo, a.a.CustomerName }).Distinct().ToList();
            var carriersListResidual = activationsResidual.Select(a => new { a.a.CarrierId }).Distinct().Join(db.Carriers, a => a.CarrierId, c => c.Id, (a, c) => new { c.Id, c.Name }).ToList();

            var customersList = customersListSpiff.Union(customersListResidual).ToList();
            var carriersList = carriersListSpiff.Union(carriersListResidual).ToList();

            // One excell report per customer
            string filePath;
            FileInfo file;
            ExcelPackage package;
            string sheetName;
            List<ExcelWorksheet> wsDealers = new List<ExcelWorksheet>();

            ExcelWorksheet wsSpiff;
            ExcelWorksheet wsResidual;
            ExcelWorksheet wsMisc;
            ExcelWorksheet wsSummaryPerCust;
            ExcelWorksheet wsSummaryPerCarrier;

            int row = 1;
            int col = 1;
            int wsCount = 1;
            List<ResumeData> resumeSpiffList = new List<ResumeData>();
            List<ResumeData> resumeResidualList = new List<ResumeData>();

            List<ReupData> reupDataList = new List<ReupData>();

            ResumeData resumeData;
            var dealerFileName = "{0} ({1})_Detailed Commission Reports ({2})"; //Next Generation Wireless (SUB1008)_Detailed Commission Reports (MM-YYYY)  
            var summaryFileName = "Commission Payout Summary ({0})"; //Commission Payout Summary (MM-YYYY)
            ResumeDealerData rd;

            foreach (var customer in customersList)
            {
                List<ResumeDealerData> resumeDealerData = new List<ResumeDealerData>();
                row = 1;
                col = 1;

                var activationsPerCust = activationsSpiffList.Where(a => a.a.CustNo == customer.CustNo).ToList();
                var residualsPerCust = activationsResidualList.Where(a => a.a.CustNo == customer.CustNo).ToList();

                if (customer.CustNo == null) filePath = Path.Combine(Server.MapPath("~/Content/Temp/"), "unknown" + ".xlsx");
                else filePath = Path.Combine(Server.MapPath("~/Content/Temp/"), String.Format(dealerFileName, String.Join("_", customer.CustomerName.Trim().Split(Path.GetInvalidFileNameChars())), customer.CustNo.Trim(), month + "-" + year) + ".xlsx");

                file = new FileInfo(filePath);
                if (file.Exists)
                {
                    file.Delete();  // ensures we create a new workbook
                }
                package = new ExcelPackage(file);

                int addCol = 1;
                double dealerCommission = 0;
                bool reup = false;

                foreach (var oper in carriersList)
                {
                    var activationsPerCarrier = activationsPerCust.Where(a => a.a.CarrierId == oper.Id).ToList();

                    reup = false;

                    if (activationsPerCarrier.Count > 0)
                    {

                        wsSpiff = package.Workbook.Worksheets.Add(oper.Name + " Spif ");

                        row = 1;
                        col = 1;
                        SetWSHeader(ref wsSpiff, ref row, ref col, customer.CustNo + " - " + customer.CustomerName);
                        
                        // spiff
                        foreach (var activation in activationsPerCarrier)
                        {
                            if (activation.a.Category != "Preloaded" && activation.a.Category != "Instant")
                            {
                                resumeData = new ResumeData();
                                resumeData.DealerId = customer.CustNo;
                                resumeData.DealerName = customer.CustomerName;

                                dealerCommission = activation.c.DealerCommissionValue;
                                col = 1;
                                row++;
                                wsSpiff.Cells[row, col].Value = activation.a.Serial;

                                col++;
                                wsSpiff.Cells[row, col].Value = activation.a.CarrierName;
                                resumeData.Carrier = activation.a.CarrierName;
                                resumeData.CarrierId = activation.a.CarrierId;
                                col++;
                                wsSpiff.Cells[row, col].Value = activation.a.ActionDate;
                                wsSpiff.Cells[row, col].Style.Numberformat.Format = @"mm\/dd\/yyyy\ hh:mm";
                                col++;
                                wsSpiff.Cells[row, col].Value = activation.a.PlanValue;
                                wsSpiff.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                                col++;
                                wsSpiff.Cells[row, col].Value = dealerCommission;//activation.a.; DealerCommission
                                wsSpiff.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                                resumeData.DealerCommission = dealerCommission;
                                resumeData.Commission = activation.a.Commission ?? 0;

                                addCol = col;
                                resumeSpiffList.Add(resumeData);
                            } else{reup = true;}
                            if (customer.CustNo == null)
                            {
                                addCol++;
                                wsSpiff.Cells[row, addCol].Value = activation.a.Sim;
                                addCol++;
                                wsSpiff.Cells[row, addCol].Value = activation.a.Esn;
                                addCol++;
                                wsSpiff.Cells[row, addCol].Value = activation.a.CardSmp;
                                addCol++;
                                wsSpiff.Cells[row, addCol].Value = activation.a.ByopActCardSmp;
                            }
                        }
                        if (reup) { reupDataList.Add(new ReupData(oper.Id, customer.CustNo));}
                        row++;
                        if (row > 4)
                        {
                            wsSpiff.Cells[row, 1].Value = "Total";
                            wsSpiff.Cells[row, 1].Style.Font.Bold = true;
                            wsSpiff.Cells[row, col].Formula = string.Format("Sum({0})", new ExcelAddress(4, col, row - 1, col).Address);
                            wsSpiff.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                            wsSpiff.Cells[row, col].Style.Font.Bold = true;
                            wsSpiff.Calculate();
                            rd = new ResumeDealerData();
                            rd.Carrier = oper.Name + " Spif ";
                            rd.Commission = Double.Parse(wsSpiff.Cells[row, col].Value.ToString());
                            resumeDealerData.Add(rd);
                            row++;
                        }
                        wsSpiff.Cells.AutoFitColumns();
                    }

                    var residualPerCarrier = residualsPerCust.Where(a => a.a.CarrierId == oper.Id).ToList();

                    wsSpiff = package.Workbook.Worksheets.Add(oper.Name + " Residual ");
                    row = 1;
                    col = 1;
                    SetWSHeader(ref wsSpiff, ref row, ref col, customer.CustNo + " - " + customer.CustomerName);

                    
                    // residual
                    foreach (var activation in residualPerCarrier)
                    {
                        //if()
                        resumeData = new ResumeData();
                        resumeData.DealerId = customer.CustNo;
                        resumeData.DealerName = customer.CustomerName;

                        dealerCommission = activation.a.PlanValue * activation.c.DealerCommissionValue / 100;
                        col = 1;
                        row++;
                        wsSpiff.Cells[row, col].Value = activation.a.Serial;

                        col++;
                        wsSpiff.Cells[row, col].Value = activation.a.CarrierName;
                        resumeData.Carrier = activation.a.CarrierName;
                        resumeData.CarrierId = activation.a.CarrierId;
                        col++;
                        wsSpiff.Cells[row, col].Value = activation.a.ActionDate;
                        wsSpiff.Cells[row, col].Style.Numberformat.Format = @"mm\/dd\/yyyy\ hh:mm";
                        col++;
                        wsSpiff.Cells[row, col].Value = activation.a.PlanValue;
                        wsSpiff.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                        col++;

                        wsSpiff.Cells[row, col].Value = dealerCommission;// DealerCommission
                        wsSpiff.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";

                        resumeData.DealerCommission = dealerCommission;
                        resumeData.Commission = activation.a.Commission ?? 0;

                        addCol = col;
                        resumeResidualList.Add(resumeData);
                        if (customer.CustNo == null)
                        {
                            addCol++;
                            wsSpiff.Cells[row, addCol].Value = activation.a.Sim;
                            addCol++;
                            wsSpiff.Cells[row, addCol].Value = activation.a.Esn;
                            addCol++;
                            wsSpiff.Cells[row, addCol].Value = activation.a.CardSmp;
                            addCol++;
                            wsSpiff.Cells[row, addCol].Value = activation.a.ByopActCardSmp;
                        }
                    }
                    row++;
                    rd = new ResumeDealerData();
                    if (row > 4)
                    {
                        wsSpiff.Cells[row, 1].Value = "Total";
                        wsSpiff.Cells[row, 1].Style.Font.Bold = true;
                        wsSpiff.Cells[row, col].Formula = string.Format("Sum({0})", new ExcelAddress(4, col, row - 1, col).Address);
                        wsSpiff.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                        wsSpiff.Cells[row, col].Style.Font.Bold = true;
                        wsSpiff.Calculate();
                        rd.Carrier = oper.Name + " Residual ";
                        rd.Commission = Double.Parse(wsSpiff.Cells[row, col].Value.ToString());
                        row++;
                    }
                    wsSpiff.Cells.AutoFitColumns();
                    if (residualPerCarrier.Count == 0 || residualPerCarrier[0].c.DealerCommissionValue == 0 || (activationsPerCarrier.Count == 0))
                    {
                        package.Workbook.Worksheets.Delete(wsSpiff);
                    }
                    else { resumeDealerData.Add(rd); }

                }
                // Summary sheet per dealer
                if (package.Workbook.Worksheets.Count > 0)
                {
                    wsSpiff = package.Workbook.Worksheets.Add("Summary");
                    row = 1;
                    col = 1;

                    wsSpiff.Cells.Style.Font.Name = "Arial";
                    wsSpiff.Cells.Style.Font.Size = 8;
                    wsSpiff.Cells[row, col].Value = customer.CustNo + " - " + customer.CustomerName;
                    wsSpiff.Cells[row, col].Style.Font.Bold = true;
                    wsSpiff.Cells[row, col, row, 2].Merge = true;
                    wsSpiff.Cells[row, col, row, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    wsSpiff.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wsSpiff.Cells[row, col].Style.Fill.BackgroundColor.SetColor(headerFillColor);
                    wsSpiff.Cells[row, col].Style.Font.Color.SetColor(headerTextColor);
                    row++;
                    row++;
                    col = 2;
                    wsSpiff.Cells[row, col].Value = "Commission";
                    wsSpiff.Cells[row, col].Style.Font.Bold = true;
                    wsSpiff.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wsSpiff.Cells[row, col].Style.Fill.BackgroundColor.SetColor(headerFillColor);
                    wsSpiff.Cells[row, col].Style.Font.Color.SetColor(headerTextColor);

                    foreach (var summary in resumeDealerData)
                    {
                        col = 1;
                        row++;
                        wsSpiff.Cells[row, col].Value = summary.Carrier;
                        col++;
                        wsSpiff.Cells[row, col].Value = summary.Commission;
                        wsSpiff.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                    }
                    row++;
                    col = 2;
                    wsSpiff.Cells[row, 1].Value = "Total";
                    wsSpiff.Cells[row, 1].Style.Font.Bold = true;
                    wsSpiff.Cells[row, col].Formula = string.Format("Sum({0})", new ExcelAddress(4, col, row - 1, col).Address);
                    wsSpiff.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";

                    wsSpiff.Cells[row, col].Style.Font.Bold = true;
                    wsSpiff.Cells.AutoFitColumns();
                    package.Workbook.Worksheets.MoveToStart("Summary");

                    package.Save();
                }
            }

            // Summary

            filePath = Path.Combine(Server.MapPath("~/Content/Temp/"), String.Format(summaryFileName, month + "-" + year) + ".xlsx");
            file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();  // ensures we create a new workbook
            }
            package = new ExcelPackage(file);
            sheetName = month + "-" + year;

            ExcelWorksheet ws1;

            package.Workbook.Worksheets.Add("No MISC " + month + "-" + year);
            wsSpiff = package.Workbook.Worksheets[1];
            package.Workbook.Worksheets.Add("MISC " + month + "-" + year);
            wsMisc = package.Workbook.Worksheets[2];
            package.Workbook.Worksheets.Add("Summary per Customer " + month + "-" + year);
            wsSummaryPerCust = package.Workbook.Worksheets[3];
            package.Workbook.Worksheets.Add("Summary per Carrier " + month + "-" + year);
            wsSummaryPerCarrier = package.Workbook.Worksheets[4];

            row = 1;
            col = 1;
            SetWSCellHeader(ref wsSpiff, row, col, "Merchant ID");
            SetWSCellHeader(ref wsMisc, row, col, "Merchant ID");
            SetWSCellHeader(ref wsSummaryPerCust, row, col, "Merchant ID");
            col++;
            SetWSCellHeader(ref wsSpiff, row, col, "Merchant Name");
            SetWSCellHeader(ref wsMisc, row, col, "Merchant Name");
            SetWSCellHeader(ref wsSummaryPerCust, row, col, "Merchant Name");

            SetWSCellHeader(ref wsSummaryPerCust, row, col + 1, "Spiff");
            SetWSCellHeader(ref wsSummaryPerCust, row, col + 2, "Residual");
            SetWSCellHeader(ref wsSummaryPerCust, row, col + 3, "Total amount due");
            SetWSCellHeader(ref wsSummaryPerCust, row, col + 4, "TM profit");

            wsSummaryPerCust.Cells[row, 1, row, col].AutoFilter = true;

            int colCopy = col;
            foreach (var oper in carriersList)
            {
                colCopy = col;
                SetWSMerchantHeader(ref wsSpiff, ref row, ref col, oper.Name);
                col = colCopy;
                SetWSMerchantHeader(ref wsMisc, ref row, ref col, oper.Name);
            }

            col++;
            SetWSCellHeader(ref wsSpiff, row, col, "Total amount due");
            SetWSCellHeader(ref wsMisc, row, col, "Total amount due");
            col++;
            SetWSCellHeader(ref wsSpiff, row, col, "Total revenue");
            SetWSCellHeader(ref wsMisc, row, col, "Total revenue");

            wsSpiff.Cells[row, 1, row, col].AutoFilter = true;
            wsMisc.Cells[row, 1, row, col].AutoFilter = true;

            var activationsResume = resumeSpiffList.GroupBy(a => new { a.DealerId, a.CarrierId }).Select(a => new { CustNo = a.Key.DealerId, a.Key.CarrierId, totalSpiff = a.Sum(x => x.DealerCommission), total = a.Sum(x => x.Commission) }).ToList();
            var residualResume = resumeResidualList.GroupBy(a => new { a.DealerId, a.CarrierId }).Select(a => new { CustNo = a.Key.DealerId, a.Key.CarrierId, totalResidual = a.Sum(x => x.DealerCommission), total = a.Sum(x => x.Commission) }).ToList();

            var actSummaryPerCustomer = resumeSpiffList.GroupBy(a => new { a.DealerId }).Select(a => new { CustNo = a.Key.DealerId, totalSpiff = a.Sum(x => x.DealerCommission), total = a.Sum(x => x.Commission) }).ToList();
            var resSummaryPerCustomer = resumeResidualList.GroupBy(a => new { a.DealerId }).Select(a => new { CustNo = a.Key.DealerId, totalResidual = a.Sum(x => x.DealerCommission), total = a.Sum(x => x.Commission) }).ToList();

            int rowMisc = row;
            int rowSummary = row;

            foreach (var customer in customersList)
            {
                col = 1;
                var carrierActivations = activationsResume.Where(a => a.CustNo == customer.CustNo).ToList();
                var carrierResidual = residualResume.Where(a => a.CustNo == customer.CustNo).ToList();

                var custActivations = actSummaryPerCustomer.Where(a => a.CustNo == customer.CustNo).FirstOrDefault();
                var custResidual = resSummaryPerCustomer.Where(a => a.CustNo == customer.CustNo).FirstOrDefault();

                rowSummary++;
                wsSummaryPerCust.Cells[rowSummary, col].Value = customer.CustNo;
                wsSummaryPerCust.Cells[rowSummary, col + 1].Value = customer.CustomerName;

                SetWSMerchantValues(ref wsSummaryPerCust, rowSummary, col + 2, custActivations == null ? 0 : custActivations.totalSpiff, defaultTextColor);
                SetWSMerchantValues(ref wsSummaryPerCust, rowSummary, col + 3, custResidual == null ? 0 : custResidual.totalResidual, defaultTextColor);

                wsSummaryPerCust.Cells[rowSummary, col + 4].Formula = string.Format("Sum({0})", new ExcelAddress(rowSummary, col + 2, rowSummary, col + 3).Address);
                wsSummaryPerCust.Cells[rowSummary, col + 4].Style.Numberformat.Format = "$ #,###,###.00";
                wsSummaryPerCust.Cells[rowSummary, col + 4].Style.Font.Bold = true;
                wsSummaryPerCust.Cells[rowSummary, col + 4].Style.Font.Italic = true;
                wsSummaryPerCust.Cells[rowSummary, col + 4].Style.Font.Color.SetColor(summaryTextColor);

                var profit = (custActivations == null ? 0 : custActivations.total - custActivations.totalSpiff) + (custResidual == null ? 0 : custResidual.total - custResidual.totalResidual);
                SetWSMerchantValues(ref wsSummaryPerCust, rowSummary, col + 5, profit, summaryTextColor);
                wsSummaryPerCust.Cells[rowSummary, col + 5].Style.Font.Bold = true;

                //                1.	**SMS**
                //2.	**$-SMS**
                //3.	**CLOSED**
                //4.	**CLOSED OWES EMIDA**
                //5.	**CLOSED PAYOUTS ONLY**
                //6.	**OWES CLOSED**
                //7.	**OWES EMIDA**
                //8.	**OWES INVOICE**
                //9.	**OWES MONEY**
                //10.	**OWES REUP**
                //11.	**OWES $**
                //12.	**OWES 2 EMIDA**
                //13.	**OWES EMIDA & REUP**
                //14.	**OWES EMIDA SINCE 2012**

                if (customer.CustomerName.Trim().Contains("**CLOSED") || customer.CustomerName.Trim().Contains("**OWES") || customer.CustomerName.Trim().Contains("**SMS**") || customer.CustomerName.Trim().Contains("**$-SMS**") || customer.CustomerName.Trim().Contains("**$-SMS**") || customer.CustomerName.Trim().Contains("**CLOSED**") || customer.CustomerName.Trim().Contains("**CLOSED OWES EMIDA**") || customer.CustomerName.Trim().Contains("**CLOSED PAYOUTS ONLY**") || customer.CustomerName.Trim().Contains("**OWES CLOSED**") || customer.CustomerName.Trim().Contains("**OWES EMIDA**") || customer.CustomerName.Trim().Contains("**OWES INVOICE**") || customer.CustomerName.Trim().Contains("**OWES MONEY**") || customer.CustomerName.Trim().Contains("**OWES REUP**") || customer.CustomerName.Trim().Contains("**OWES $**") || customer.CustomerName.Trim().Contains("**OWES 2 EMIDA**") || customer.CustomerName.Trim().Contains("**OWES EMIDA & REUP**") || customer.CustomerName.Trim().Contains("**OWES EMIDA SINCE 2012**"))
                {
                    rowMisc++;
                    wsMisc.Cells[rowMisc, col].Value = customer.CustNo;
                    col++;
                    wsMisc.Cells[rowMisc, col].Value = customer.CustomerName;

                    foreach (var oper in carriersList)
                    {
                        var data = carrierActivations.Where(a => a.CarrierId == oper.Id).FirstOrDefault();
                        col++;
                        SetWSMerchantValues(ref wsMisc, rowMisc, col, data == null ? 0 : data.total, defaultTextColor);
                        wsMisc.Cells[rowMisc, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        wsMisc.Cells[rowMisc, col].Style.Fill.BackgroundColor.SetColor(columnMasterFillColor);
                        col++;
                        SetWSMerchantValues(ref wsMisc, rowMisc, col, data == null ? 0 : data.totalSpiff, defaultTextColor);
                        col++;
                        SetWSMerchantValues(ref wsMisc, rowMisc, col, data == null ? 0 : data.total - data.totalSpiff, summaryTextColor);
                        wsMisc.Cells[rowMisc, col].Style.Font.Italic = true;

                        if (reupDataList.Exists(a=>a.CustNo == customer.CustNo && a.Carrier == oper.Id)) // activationsPerCarrier.Count == 0) wsSpiff.Cells[row, col].Style.Font.Color.SetColor(ceroValueTextColor);

                        var data1 = carrierResidual.Where(a => a.CarrierId == oper.Id).FirstOrDefault();
                        col++;
                        SetWSMerchantValues(ref wsMisc, rowMisc, col, data1 == null ? 0 : data1.total, defaultTextColor);
                        wsMisc.Cells[rowMisc, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        wsMisc.Cells[rowMisc, col].Style.Fill.BackgroundColor.SetColor(columnMasterFillColor);
                        col++;
                        SetWSMerchantValues(ref wsMisc, rowMisc, col, data1 == null ? 0 : data1.totalResidual, defaultTextColor);
                        col++;
                        SetWSMerchantValues(ref wsMisc, rowMisc, col, data1 == null ? 0 : data1.total - data1.totalResidual, summaryTextColor);
                        wsMisc.Cells[rowMisc, col].Style.Font.Italic = true;

                        SetWSMerchantValues(ref wsMisc, rowMisc, carriersList.Count() * 6 + 3, (wsMisc.Cells[rowMisc, carriersList.Count() * 6 + 3].Value == null ? 0 : Double.Parse(wsMisc.Cells[rowMisc, carriersList.Count() * 6 + 3].Value.ToString())) + (data == null ? 0 : data.totalSpiff) + (data1 == null ? 0 : data1.totalResidual), summaryTextColor);
                        wsMisc.Cells[rowMisc, carriersList.Count() * 6 + 3].Style.Font.Bold = true;
                        SetWSMerchantValues(ref wsMisc, rowMisc, carriersList.Count() * 6 + 4, (wsMisc.Cells[rowMisc, carriersList.Count() * 6 + 4].Value == null ? 0 : Double.Parse(wsMisc.Cells[rowMisc, carriersList.Count() * 6 + 4].Value.ToString())) + (data == null ? 0 : data.total - data.totalSpiff) + (data1 == null ? 0 : data1.total - data1.totalResidual), summaryTextColor);
                        wsMisc.Cells[rowMisc, carriersList.Count() * 6 + 4].Style.Font.Bold = true;
                    }
                }
                else
                {
                    row++;
                    wsSpiff.Cells[row, col].Value = customer.CustNo;
                    col++;
                    wsSpiff.Cells[row, col].Value = customer.CustomerName;

                    foreach (var oper in carriersList)
                    {
                        resumeSpiffList.Where(a => a.c);
                        var data = carrierActivations.Where(a => a.CarrierId == oper.Id).FirstOrDefault();
                        col++;
                        SetWSMerchantValues(ref wsSpiff, row, col, data == null ? 0 : data.total, defaultTextColor);
                        wsSpiff.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        wsSpiff.Cells[row, col].Style.Fill.BackgroundColor.SetColor(columnMasterFillColor);
                        col++;
                        SetWSMerchantValues(ref wsSpiff, row, col, data == null ? 0 : data.totalSpiff, defaultTextColor);
                        col++;
                        SetWSMerchantValues(ref wsSpiff, row, col, data == null ? 0 : data.total - data.totalSpiff, summaryTextColor);
                        wsSpiff.Cells[row, col].Style.Font.Italic = true;

                        var data1 = carrierResidual.Where(a => a.CarrierId == oper.Id).FirstOrDefault();
                        col++;
                        SetWSMerchantValues(ref wsSpiff, row, col, data1 == null ? 0 : data1.total, defaultTextColor);
                        wsSpiff.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        wsSpiff.Cells[row, col].Style.Fill.BackgroundColor.SetColor(columnMasterFillColor);

                        col++;
                        //if (wsSpiff.Cells[row,col-2].Value == "0") and data1.
                        SetWSMerchantValues(ref wsSpiff, row, col, data1 == null ? 0 : data1.totalResidual, defaultTextColor);
                        col++;
                        SetWSMerchantValues(ref wsSpiff, row, col, data1 == null ? 0 : data1.total - data1.totalResidual, summaryTextColor);
                        wsSpiff.Cells[row, col].Style.Font.Italic = true;

                        SetWSMerchantValues(ref wsSpiff, row, carriersList.Count() * 6 + 3, (wsSpiff.Cells[row, carriersList.Count() * 6 + 3].Value == null ? 0 : Double.Parse(wsSpiff.Cells[row, carriersList.Count() * 6 + 3].Value.ToString())) + (data == null ? 0 : data.totalSpiff) + (data1 == null ? 0 : data1.totalResidual), summaryTextColor);
                        wsSpiff.Cells[row, carriersList.Count() * 6 + 3].Style.Font.Bold = true;
                        SetWSMerchantValues(ref wsSpiff, row, carriersList.Count() * 6 + 4, (wsSpiff.Cells[row, carriersList.Count() * 6 + 4].Value == null ? 0 : Double.Parse(wsSpiff.Cells[row, carriersList.Count() * 6 + 4].Value.ToString())) + (data == null ? 0 : data.total - data.totalSpiff) + (data1 == null ? 0 : data1.total - data1.totalResidual), summaryTextColor);
                        wsSpiff.Cells[row, carriersList.Count() * 6 + 4].Style.Font.Bold = true;
                    }
                }

            }
            row++;
            rowMisc++;
            rowSummary++;
            wsSpiff.Cells[row, 1].Value = "Total";
            wsSpiff.Cells[row, 1].Style.Font.Bold = true;
            wsMisc.Cells[rowMisc, 1].Value = "Total";
            wsMisc.Cells[rowMisc, 1].Style.Font.Bold = true;
            wsSummaryPerCust.Cells[rowSummary, 1].Value = "Total";
            wsSummaryPerCust.Cells[rowSummary, 1].Style.Font.Bold = true;

            for (int i = 3; i <= col + 2; i++)
            {
                wsSpiff.Cells[row, i].Formula = string.Format("Sum({0})", new ExcelAddress(2, i, row - 1, i).Address);
                wsSpiff.Cells[row, i].Style.Numberformat.Format = "$ #,###,###.00";
                wsSpiff.Cells[row, i].Style.Font.Bold = true;
                wsSpiff.Cells[row, i].Style.Font.Italic = true;

                wsMisc.Cells[rowMisc, i].Formula = string.Format("Sum({0})", new ExcelAddress(2, i, rowMisc - 1, i).Address);
                wsMisc.Cells[rowMisc, i].Style.Numberformat.Format = "$ #,###,###.00";
                wsMisc.Cells[rowMisc, i].Style.Font.Bold = true;
                wsMisc.Cells[rowMisc, i].Style.Font.Italic = true;
            }


            for (int i = 3; i <= 6; i++)
            {
                wsSummaryPerCust.Cells[rowSummary, i].Formula = string.Format("Sum({0})", new ExcelAddress(2, i, rowSummary - 1, i).Address);
                wsSummaryPerCust.Cells[rowSummary, i].Style.Numberformat.Format = "$ #,###,###.00";
                wsSummaryPerCust.Cells[rowSummary, i].Style.Font.Bold = true;
                wsSummaryPerCust.Cells[rowSummary, i].Style.Font.Italic = true;
            }

            wsSpiff.Cells.AutoFitColumns();
            wsMisc.Cells.AutoFitColumns();
            wsSummaryPerCust.Cells.AutoFitColumns();

            //summary per carrier
            col = 1;
            row = 1;
            var actSummaryPerCarrier = resumeSpiffList.GroupBy(a => new { a.CarrierId }).Select(a => new { CarrierId = a.Key.CarrierId, totalSpiff = a.Sum(x => x.DealerCommission), total = a.Sum(x => x.Commission) }).ToList();
            var resSummaryPerCarrier = resumeResidualList.GroupBy(a => new { a.CarrierId }).Select(a => new { CarrierId = a.Key.CarrierId, totalResidual = a.Sum(x => x.DealerCommission), total = a.Sum(x => x.Commission) }).ToList();

            SetWSCellHeader(ref wsSummaryPerCarrier, row, col, "Carrier");
            SetWSCellHeader(ref wsSummaryPerCarrier, row, col + 1, "Merchant spiff");
            SetWSCellHeader(ref wsSummaryPerCarrier, row, col + 2, "TM spiff");
            SetWSCellHeader(ref wsSummaryPerCarrier, row, col + 3, "Merchant residual");
            SetWSCellHeader(ref wsSummaryPerCarrier, row, col + 4, "TM residual");
            SetWSCellHeader(ref wsSummaryPerCarrier, row, col + 5, "Total due");
            SetWSCellHeader(ref wsSummaryPerCarrier, row, col + 6, "TM profit");

            foreach (var oper in carriersList)
            {
                col = 1;
                row++;
                SetWSCellHeader(ref wsSummaryPerCarrier, row, col, oper.Name);
                var data = actSummaryPerCarrier.Where(a => a.CarrierId == oper.Id).FirstOrDefault();
                var data1 = resSummaryPerCarrier.Where(a => a.CarrierId == oper.Id).FirstOrDefault();
                col++;
                SetWSMerchantValues(ref wsSummaryPerCarrier, row, col, data == null ? 0 : data.totalSpiff, defaultTextColor);
                col++;
                SetWSMerchantValues(ref wsSummaryPerCarrier, row, col, data == null ? 0 : data.total - data.totalSpiff, defaultTextColor);
                col++;
                SetWSMerchantValues(ref wsSummaryPerCarrier, row, col, data1 == null ? 0 : data1.totalResidual, defaultTextColor);
                col++;
                SetWSMerchantValues(ref wsSummaryPerCarrier, row, col, data1 == null ? 0 : data1.total - data1.totalResidual, defaultTextColor);
                col++;
                SetWSMerchantValues(ref wsSummaryPerCarrier, row, col, (data == null ? 0 : data.totalSpiff) + (data1 == null ? 0 : data1.totalResidual), summaryTextColor);
                wsSummaryPerCarrier.Cells[row, col].Style.Font.Italic = true;
                col++;
                SetWSMerchantValues(ref wsSummaryPerCarrier, row, col, (data == null ? 0 : data.total - data.totalSpiff) + (data1 == null ? 0 : data1.total - data1.totalResidual), summaryTextColor);
                wsSummaryPerCarrier.Cells[row, col].Style.Font.Italic = true;
            }
            row++;
            for (int i = 2; i <= 7; i++)
            {
                wsSummaryPerCarrier.Cells[row, i].Formula = string.Format("Sum({0})", new ExcelAddress(2, i, row - 1, i).Address);
                wsSummaryPerCarrier.Cells[row, i].Style.Numberformat.Format = "$ #,###,###.00";
                wsSummaryPerCarrier.Cells[row, i].Style.Font.Bold = true;
                wsSummaryPerCarrier.Cells[row, i].Style.Font.Italic = true;
            }

            wsSummaryPerCarrier.Cells.AutoFitColumns();
            package.Save();

        }
        private void AddTotalSummary(ref ExcelWorksheet ws, List<ResumeData> data)
        {
            //var resume = data.GroupBy(a=>a.);
        }

        private void SetWSMerchantValues(ref ExcelWorksheet ws, int row, int col, double value, System.Drawing.Color color)
        {
            ws.Cells[row, col].Value = value;
            ws.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
            ws.Cells[row, col].Style.Font.Color.SetColor(color);

            //if (value != 0) ws.Cells[row, col].Style.Font.Color.SetColor(color);
            //else ws.Cells[row, col].Style.Font.Color.SetColor(ceroValueTextColor);
        }

        private void SetWSMerchantHeader(ref ExcelWorksheet ws, ref int row, ref int col, string title)
        {
            col++;
            SetWSCellHeader(ref ws, row, col, title + "_MASTER Spiff");
            col++;
            SetWSCellHeader(ref ws, row, col, title + "_MERCHANT Spiff");
            col++;
            SetWSCellHeader(ref ws, row, col, "TM PROFIT_" + title + "_Spiff");
            col++;
            SetWSCellHeader(ref ws, row, col, title + "_MASTER Residual");
            col++;
            SetWSCellHeader(ref ws, row, col, title + "_MERCHANT Residual");
            col++;
            SetWSCellHeader(ref ws, row, col, "TM PROFIT_" + title + "_Residual");
        }

        private void SetWSCellHeader(ref ExcelWorksheet ws, int row, int col, string title)
        {
            ws.Cells[row, col].Value = title;
            ws.Cells[row, col].Style.Font.Bold = true;
            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(headerFillColor);
            ws.Cells[row, col].Style.Font.Color.SetColor(headerTextColor);
        }

        private void SetWSHeader(ref ExcelWorksheet ws, ref int row, ref int col, string title)
        {
            ws.Cells.Style.Font.Name = "Arial";
            ws.Cells.Style.Font.Size = 8;
            ws.Cells[row, col].Value = title;
            ws.Cells[row, col].Style.Font.Bold = true;
            ws.Cells[row, col, row, 5].Merge = true;
            ws.Cells[row, col, row, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(headerFillColor);
            ws.Cells[row, col].Style.Font.Color.SetColor(headerTextColor);

            row++;
            row++;
            ws.Cells[row, col].Value = "Serial";
            ws.Cells[row, col].Style.Font.Bold = true;
            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(headerFillColor);
            ws.Cells[row, col].Style.Font.Color.SetColor(headerTextColor);
            col++;
            ws.Cells[row, col].Value = "Carrier";
            ws.Cells[row, col].Style.Font.Bold = true;
            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(headerFillColor);
            ws.Cells[row, col].Style.Font.Color.SetColor(headerTextColor);
            col++;
            ws.Cells[row, col].Value = "Action Date";
            ws.Cells[row, col].Style.Font.Bold = true;
            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(headerFillColor);
            ws.Cells[row, col].Style.Font.Color.SetColor(headerTextColor);
            col++;
            ws.Cells[row, col].Value = "Plan";
            ws.Cells[row, col].Style.Font.Bold = true;
            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(headerFillColor);
            ws.Cells[row, col].Style.Font.Color.SetColor(headerTextColor);
            col++;
            ws.Cells[row, col].Value = "Commission";
            ws.Cells[row, col].Style.Font.Bold = true;
            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(headerFillColor);
            ws.Cells[row, col].Style.Font.Color.SetColor(headerTextColor);
            ws.Cells[row, 1, row, col].AutoFilter = true;

        }

        private void CreateGeneralSpiffReport()
        {
            //select * from ActivationReports, Customers, (select Operators.id as id, Plans.id as PlanId, Value, PlanCommissionValue, DealerCommissionValue from Plans, DefaultPlanCommissions, Operators
            //where DefaultPlanCommissions.PlanId = Plans.Id
            //and Operators.Id = Plans.OperatorId and Operators.Id=1) as PlansValue
            //where ActivationReports.CustomerId = Customers.Id 
            //and ActivationReports.PlanId = PlansValue.PlanId

            int month = 10;
            int year = 2014;

            var customersSpiff = db.Customers.Select(c => new { c.CustNo, MinValue = c.PlanCommissions.Select(p => p.MinValue), MaxValue = c.PlanCommissions.Select(p => p.MaxValue), DealerCommissionValue = c.PlanCommissions.Select(p => p.DealerCommissionValue), CommissionType = c.PlanCommissions.Select(p => p.CommissionType) });
            var activations = customersSpiff.Join(db.ActivationReports, c => c.CustNo, a => a.CustNo, (c, a) => new { a, c });//.Where(s => s.a.PlanValue > s.c.MinValue.) && s.a.PlanValue <= s.PlanCommissions.Select(p=>p.MaxValue).FirstOrDefault());//a=>a.PlanCommissions.Select(p=>p.MinValue).FirstOrDefault()));
            //var activationsByPlanValue = activations.Select(a => new { a.a.Serial, a.a.PlanValue, a.a.CustNo, a.c.DealerCommissionValue, a.c.MinValue, a.c.MaxValue }).Where(p => p.MinValue.Any(x => x < p.PlanValue) && p.MaxValue.Any(x => x > p.PlanValue)).ToList();

            var activationsList = activations.OrderBy(a => a.a.CustNo).ToList();
            var customersListSpiff = activations.Select(a => new { a.a.CustNo, a.a.CustomerName }).Distinct().ToList();
            var carriersListSpiff = activations.Select(a => new { a.a.CarrierId }).Distinct().Join(db.Carriers, a => a.CarrierId, c => c.Id, (a, c) => new { c.Id, c.Name }).ToList();

            //var activationsResume = customers1.Join(db.ActivationReports, c => c.CustNo, a => a.CustNo, (c, a) => new { c, a }).GroupBy(a => new { a.a.CustNo, a.a.CarrierId }).Select(a => new { a.Key.CustNo, a.Key.CarrierId, totalSpiff = a.Sum(x => x.c.PlanCommissions.Select(s => s.DealerCommissionValue).FirstOrDefault()) });

            //var residuals = db.ResidualReports;
            //var residualsList = residuals.OrderBy(a => a.CustNo).ToList();
            //var customersListResidual = residuals.Select(a => new { a.CustNo, a.CustomerName }).Distinct().ToList();
            //var carriersListResidual = residuals.Select(a => new { a.CarrierId }).Distinct().Join(db.Carriers, a => a.CarrierId, c => c.Id, (a, c) => new { c.Id, c.Name }).ToList();
            //var residualResume = customers1.Join(db.ResidualReports, c => c.CustNo, a => a.CustNo, (c, a) => new { c, a }).GroupBy(a => new { a.a.CustNo, a.a.CarrierId }).Select(a => new { a.Key.CustNo, a.Key.CarrierId, totalResidual = a.Sum(x => x.c.PlanCommissions.Select(s => s.DealerCommissionValue).FirstOrDefault()) });

            //var customersList = customersListSpiff.Union(customersListResidual).ToList();
            //var carriersList = carriersListSpiff.Union(carriersListResidual).ToList();


            //var plans = db.Carriers.Join(db.Plans, o => o.Id, p => p.CarrierId, (o, p) => new { o, p });
            //    .Join(db.DefaultPlanCommissions, dpp => dpp.p.Id, dp => dp.PlanId, (dpp, dp) => new { dpp, dp })
            //   .Select(m => new { CarrierId = m.dpp.o.Id, CarrierName = m.dpp.o.Name, PlanName = m.dpp.p.Name, PlanId = m.dpp.p.Id, m.dp.PlanCommissionValue, m.dp.DealerCommissionValue, m.dp.Plan.Value });
            //var activations = plans.Join(db.ActivationReports, ps => new { ps.CarrierId, ps.PlanId }, a => new { a.CarrierId, a.PlanId}, (ps, a) => new { ps.CarrierName, ps.CarrierId, a.CustNo, a.CustomerName, ps.PlanName, ps.Value, ps.DealerCommissionValue, a.Serial, a.Sim, a.Esn, a.CardSmp, a.ByopActCardSmp, a.ActionDate });

            //var activationsList = activations.OrderBy(a => a.CustNo).ToList();
            //var customersListSpiff = activations.Select(a => new { a.CustNo, a.CustomerName }).Distinct().ToList();
            //var carriersListSpiff = activations.Select(a => new { a.CarrierId, a.CarrierName }).Distinct().ToList();

            //var activationsResume = activations.GroupBy(a => new { a.CustNo, a.CarrierId }).Select(a => new { a.Key.CustNo, a.Key.CarrierId, totalSpiff = a.Sum(x => x.DealerCommissionValue) }).ToList();

            //var residuals = plans.Join(db.ResidualReports, ps => new { ps.CarrierId, ps.PlanId }, a => new { a.CarrierId, a.PlanId }, (ps, a) => new { ps.CarrierName, ps.CarrierId, a.CustNo, a.CustomerName, ps.PlanName, ps.Value, ps.DealerCommissionValue, a.Serial, a.Sim, a.Esn, a.CardSmp, a.ByopActCardSmp, a.ActionDate });
            //var customersListResidual = residuals.Select(a => new { a.CustNo, a.CustomerName }).Distinct().ToList();
            //var carriersListResidual = residuals.Select(a => new { a.CarrierId, a.CarrierName }).Distinct().ToList();

            //var residualsList = residuals.OrderBy(a => a.CustNo).ToList();
            //var residualResume = residuals.GroupBy(a => new { a.CustNo, a.CarrierId }).Select(a => new { a.Key.CustNo, a.Key.CarrierId, totalSpiff = a.Sum(x => x.DealerCommissionValue) }).ToList();

            //var customersList = customersListSpiff.Union(customersListResidual).ToList();
            //var carriersList = carriersListSpiff.Union(carriersListResidual).ToList();

            // One excell report per customer
            string filePath;
            FileInfo file;
            ExcelPackage package;
            string sheetName;
            ExcelWorksheet ws1;
            int row = 1;
            int col = 1;
            List<ResumeData> resumeDataList = new List<ResumeData>();
            ResumeData resumeData;

            foreach (var customer in customersListSpiff)
            {
                var activationsPerCust = activationsList.Where(a => a.a.CustNo == customer.CustNo).ToList();
                // var residualsPerCust = residualsList.Where(a => a.CustNo == customer.CustNo).ToList();

                if (customer.CustNo == null) filePath = Path.Combine(Server.MapPath("~/Content/Temp/"), "unknown" + ".xlsx");
                else filePath = Path.Combine(Server.MapPath("~/Content/Temp/"), customer.CustNo.Trim() + ".xlsx");

                file = new FileInfo(filePath);
                if (file.Exists)
                {
                    file.Delete();  // ensures we create a new workbook
                }
                package = new ExcelPackage(file);
                sheetName = "Date MM/YYYY " + month + "/" + year;
                package.Workbook.Worksheets.Add(sheetName);
                ws1 = package.Workbook.Worksheets[1];

                row = 1;
                col = 1;
                ws1.Cells.Style.Font.Name = "Arial";
                ws1.Cells.Style.Font.Size = 8;
                ws1.Cells[row, col].Value = customer.CustNo + " - " + customer.CustomerName;
                ws1.Cells[row, col].Style.Font.Bold = true;
                ws1.Cells[row, col, row, 5].Merge = true;
                ws1.Cells[row, col, row, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                row++;
                row++;
                ws1.Cells[row, col].Value = "Serial";
                col++;
                ws1.Cells[row, col].Value = "Carrier";
                col++;
                ws1.Cells[row, col].Value = "Action Date";
                col++;
                ws1.Cells[row, col].Value = "Plan";
                col++;
                ws1.Cells[row, col].Value = "Commission";
                ws1.Cells[row, 1, row, col].AutoFilter = true;

                int addCol = 1;
                double dealerCommission = 0;

                foreach (var activation in activationsPerCust)
                {
                    resumeData = new ResumeData();
                    resumeData.DealerId = customer.CustNo;
                    resumeData.DealerName = customer.CustomerName;

                    var minValues = activation.c.MinValue.ToArray();
                    var maxValues = activation.c.MaxValue.ToArray();
                    var commissionType = activation.c.CommissionType.ToArray();

                    for (int i = 0; i < minValues.Count(); i++)
                    {
                        if (activation.a.PlanValue > minValues[i] && activation.a.PlanValue < maxValues[i] && commissionType[i] == Model.CommissionType.Spiff)
                        {
                            dealerCommission = activation.c.DealerCommissionValue.ToArray()[i];
                        }
                    }

                    col = 1;
                    row++;
                    ws1.Cells[row, col].Value = activation.a.Serial;

                    col++;
                    ws1.Cells[row, col].Value = activation.a.CarrierName;
                    resumeData.Carrier = activation.a.CarrierName;
                    resumeData.CarrierId = activation.a.CarrierId;
                    col++;
                    ws1.Cells[row, col].Value = activation.a.ActionDate;
                    ws1.Cells[row, col].Style.Numberformat.Format = @"mm\/dd\/yyyy\ hh:mm";
                    col++;
                    ws1.Cells[row, col].Value = activation.a.PlanValue;
                    ws1.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                    col++;
                    ws1.Cells[row, col].Value = dealerCommission;//activation.a.; DealerCommission
                    ws1.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                    resumeData.Commission = dealerCommission;

                    addCol = col;
                    resumeDataList.Add(resumeData);
                    if (customer.CustNo == null)
                    {
                        addCol++;
                        ws1.Cells[row, addCol].Value = activation.a.Sim;
                        addCol++;
                        ws1.Cells[row, addCol].Value = activation.a.Esn;
                        addCol++;
                        ws1.Cells[row, addCol].Value = activation.a.CardSmp;
                        addCol++;
                        ws1.Cells[row, addCol].Value = activation.a.ByopActCardSmp;
                    }
                }
                row++;
                if (row > 4)
                {
                    ws1.Cells[row, 1].Value = "Total";
                    ws1.Cells[row, 1].Style.Font.Bold = true;
                    ws1.Cells[row, col].Formula = string.Format("Sum({0})", new ExcelAddress(4, col, row - 1, col).Address);
                    ws1.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                    ws1.Cells[row, col].Style.Font.Bold = true;
                    row++;
                }

                //foreach (var residual in residualsPerCust)
                //{
                //    col = 1;
                //    row++;
                //    ws1.Cells[row, col].Value = residual.Serial;
                //    col++;
                //    ws1.Cells[row, col].Value = residual.CarrierName;
                //    col++;
                //    ws1.Cells[row, col].Value = residual.ActionDate;
                //    ws1.Cells[row, col].Style.Numberformat.Format = @"mm\/dd\/yyyy\ hh:mm";
                //    col++;
                //    ws1.Cells[row, col].Value = residual;
                //    ws1.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                //    col++;
                //    ws1.Cells[row, col].Value = residual.DealerCommissionValue;
                //    ws1.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                //    addCol = col;
                //    if (customer.CustNo == null)
                //    {
                //        addCol++;
                //        ws1.Cells[row, addCol].Value = residual.Sim;
                //        addCol++;
                //        ws1.Cells[row, addCol].Value = residual.Esn;
                //        addCol++;
                //        ws1.Cells[row, addCol].Value = residual.CardSmp;
                //        addCol++;
                //        ws1.Cells[row, addCol].Value = residual.ByopActCardSmp;
                //    }

                //}

                //row++;
                //ws1.Cells[row, 1].Value = "Total";
                //ws1.Cells[row, 1].Style.Font.Bold = true;
                //ws1.Cells[row, col].Formula = string.Format("Sum({0})", new ExcelAddress(4, col, row - 1, col).Address);
                //ws1.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                //ws1.Cells[row, col].Style.Font.Bold = true;

                ws1.Cells.AutoFitColumns();
                package.Save();

            }


            filePath = Path.Combine(Server.MapPath("~/Content/Temp/"), "GeneralSpiff" + ".xlsx");
            file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();  // ensures we create a new workbook
            }
            package = new ExcelPackage(file);
            sheetName = "Resumen - Date MM/YYYY " + month + "/" + year;


            package.Workbook.Worksheets.Add("Resume - " + sheetName);
            ws1 = package.Workbook.Worksheets[1];
            //package.Workbook.Worksheets.Add("All Dealers - Activation - " + sheetName);
            //ExcelWorksheet ws1 = package.Workbook.Worksheets[2];

            //CreateSheetHeader(ws1, sheetName, columnNames);

            row = 1;
            col = 1;
            ws1.Cells[row, col].Value = "Dealer ID";
            ws1.Cells[row, col].Style.Font.Bold = true;
            col++;
            ws1.Cells[row, col].Value = "Dealer Name";
            ws1.Cells[row, col].Style.Font.Bold = true;

            foreach (var oper in carriersListSpiff)
            {
                col++;
                ws1.Cells[row, col].Value = oper.Name + "_spiff";
                ws1.Cells[row, col].Style.Font.Bold = true;
                col++;
                ws1.Cells[row, col].Value = oper.Name + "_residual";
                ws1.Cells[row, col].Style.Font.Bold = true;
            }

            ws1.Cells[row, 1, row, col].AutoFilter = true;

            var activationsResume = resumeDataList.GroupBy(a => new { a.DealerId, a.CarrierId }).Select(a => new { CustNo = a.Key.DealerId, a.Key.CarrierId, totalSpiff = a.Sum(x => x.Commission) }).ToList();

            foreach (var customer in customersListSpiff)
            {
                row++;
                col = 1;
                var carrierActivations = activationsResume.Where(a => a.CustNo == customer.CustNo).ToList();
                //var carrierResidual = residualResume.Where(a => a.CustNo == customer.CustNo).ToList();

                ws1.Cells[row, col].Value = customer.CustNo;
                col++;
                ws1.Cells[row, col].Value = customer.CustomerName;

                foreach (var oper in carriersListSpiff)
                {
                    col++;
                    var data = carrierActivations.Where(a => a.CarrierId == oper.Id).FirstOrDefault();
                    ws1.Cells[row, col].Value = data == null ? 0 : data.totalSpiff;
                    ws1.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                    col++;
                    //data = carrierResidual.Where(a => a.CarrierId == oper.CarrierId).FirstOrDefault();
                    //ws1.Cells[row, col].Value = data == null ? 0 : data.totalSpiff;
                    //ws1.Cells[row, col].Style.Numberformat.Format = "$ #,###,###.00";
                }
            }
            ws1.Cells.AutoFitColumns();


            //foreach (var item in Resume)
            //{

            //    operatorName = item.OperatorName;
            //    while (operatorName != item.OperatorName)
            //    { 

            //    }
            //    initGroupRow = row + 1;
            //    row++;
            //    ws1.Cells[row, 1].Value = item.CustNo;
            //    ws1.Cells[row, 2].Value = item.totalSpiff;


            //    row++;
            //    ws1.Cells[row, 1].Value = "Total";
            //    ws1.Cells[row, 1].Style.Font.Bold = true;
            //    ws1.Cells[row, 3].Formula = string.Format("Sum({0})", new ExcelAddress(initGroupRow, 3, row - 1, 3).Address);
            //    ws1.Cells[row, 3].Style.Font.Bold = true;

            //    //ws1.Cells[row, 1].Value = "Total";
            //    //ws1.Cells[row, 3].Value = 30;//.Formula = "Sum(" + ws.Cells[3, colIndex].Address + ":" + ws.Cells[rowIndex - 1, colIndex].Address + ")";
            //}

            package.Save();

            //ActivationReport report = await db.ActivationReports.Include("Category").Include("Image").Include("Article").FirstOrDefaultAsync(s => s.Id == id);

        }

        //public List<String> GetReportDataBySerial(int serial)
        //{
        //    OleDbConnection yourConnectionHandler = new OleDbConnection("Provider=VFPOLEDB.1;Data Source=D:\\ERPFOXPRO\\EzCellERP\\Data\\ezcellerp.dbc");
        //    yourConnectionHandler.Open();

        //    if (yourConnectionHandler.State == ConnectionState.Open)
        //    {
        //        OleDbDataAdapter DA = new OleDbDataAdapter();
        //        string mySQL = "select artran.*, arcust.* "
        //                    + " from artran "
        //                    + "      join arcust "
        //                    + "         on artran.serial = arcust.serial"
        //                    + " where artran.serial=" + serial;

        //        OleDbCommand MyQuery = new OleDbCommand(mySQL, yourConnectionHandler);
        //        DataTable YourResultSet = new DataTable();
        //        DA.SelectCommand = MyQuery;

        //        DA.Fill(YourResultSet);
        //    }

        //    List<String> result = new List<string>();
        //    return result;
        //}


    }


}