using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;


namespace ERPComisions.ViewModels
{
    public class ActivationData
    {
        public string Sim{ get; set; }
        public string Esn { get; set; }
        public string CardSmp { get; set; }
        public string ByopActCardSmp { get; set; }
        public string Plan { get; set; }
        public string Commission { get; set; }
        public string ActiondDate { get; set; }
    }

    public class ImportExcelViewModel
    {

            public IList<ActivationData> ExcelData { get; set; }
    }

    public class PreviewExcelViewModel
    {
        [Range(1, 10)]
        public int excellSheetNumber { get; set; }
        public List<string> htmlReports { get; set; }
        public List<List<string>> Data { get; set; }
    }

}