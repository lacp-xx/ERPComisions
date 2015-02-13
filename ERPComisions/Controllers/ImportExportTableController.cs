using ERPComisions.ViewModels;
using ERPCommissions.ImportExportUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERPComisions.Controllers
{
    public class ImportExportTableController : Controller
    {
        // GET: ImportExportTable/Index
        public ActionResult Index()
        {
            var vm = new ImportExportTable();
            ImportFromVFP importUtil = new ImportFromVFP();
            //importUtil.ImportSpiffCommissionsStructure(1);
            importUtil.ImportSpiffCommissionsStructure(3);
            importUtil.ImportResidualCommissionsStructure(3);
            importUtil.ImportSpiffCommissionsStructure(6);
            importUtil.ImportResidualCommissionsStructure(6);
            return View(vm);
        }

         // POST: ImportExportTable/Import
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Import(ImportExportTable vm){
            ImportFromVFP importUtil = new ImportFromVFP();
            importUtil.ImportExportTable(vm.TableName);

            return RedirectToAction("Index");
        }
    }
}