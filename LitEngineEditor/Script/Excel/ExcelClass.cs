using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;

namespace ExportTool
{
    public class ExcelData
    {
        static public int sStartLine = 4;
        static public int sCSLine = 0;
        static public int sContext = 1;
        static public int sTypeLine = 2;
        static public int sFieldNameLine = 3;
        static public string sNeedType = "c";
        public string name;
        public int c;
        public int r;
        public string[,] objects;
        private bool inited = false;

        public bool IsNeed(int col)
        {
            string tcs = objects[sCSLine, col];
            return !string.IsNullOrEmpty(tcs) && tcs.Contains(sNeedType);
        }
        public void ReadExcelToArray(ISheet _sheet)
        {
            if (inited) return;
            name = _sheet.SheetName;
            IRow firstRow = _sheet.GetRow(0);
            c = firstRow.Cells.Count;
            r = _sheet.PhysicalNumberOfRows;
            objects = new string[r, c];
            int reali = 0;
            for (int i = 0; i < r; i++)
            {
                IRow trow = _sheet.GetRow(reali);
                if (trow == null) continue;
                bool tishave = false;
                for (int j = 0; j < c; j++)
                {
                    objects[reali, j] = "";
                    ICell tcell = trow.GetCell(j);
                    if (tcell == null) continue;
                    if (!tishave)
                        tishave = true;
                    objects[reali, j] = tcell.ToString().Trim();
                }
                if (tishave)
                    reali++;
            }
            inited = true;
            r = reali;
        }
    }
    class ExcelClass
    {
        //   private Excel._Application excelApp;
        private string fileName = string.Empty;
        // private Excel.Workbook wbclass;
        private string savepath;

        private FileStream mFile = null;
        private HSSFWorkbook mWorkbook;

        
        public ExcelClass(string _filename, string _savepath)
        {
            fileName = _filename;
            savepath = _savepath;

            try
            {
                mFile = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fileName.IndexOf(".xls") > 0) // 2003版本
                    mWorkbook = new HSSFWorkbook(mFile);
                else
                    DLog.LogError("只支持xls格式。");
            }
            catch (Exception ex)
            {
                DLog.LogError("Exception: " + ex.Message);
                if (mFile != null)
                    mFile.Close();
            }
        }

        public List<string> GetSheetNames()
        {
            List<string> list = new List<string>();
            int tlen = mWorkbook.NumberOfSheets;
            for (int i = 0; i < tlen; i++)
            {
                ISheet tsheet = mWorkbook[i];
                list.Add(tsheet.SheetName);
            }
            return list;
        }
        public ISheet GetWorksheetByName(string name)
        {
            return mWorkbook.GetSheet(name);
        }

        public ExcelData GetContentHaveValue(string sheetName)
        {
            ExcelData ret = new ExcelData();
            ISheet sheet = GetWorksheetByName(sheetName);
            if (sheet != null)
            {
                ret.ReadExcelToArray(sheet);
            }
            else
            {
                ret.c = 0;
                ret.r = 0;
            }
            return ret;
        }

        public void Close()
        {
            if (mWorkbook != null)
                mWorkbook.Close();
            if (mFile != null)
                mFile.Close();

        }

        public void SaveToJson()
        {
            List<string> tsheetnames = GetSheetNames();
            foreach (string tname in tsheetnames)
            {
                ExcelData tdata = GetContentHaveValue(tname);
                string tfullname = savepath + "/" + tname + ".json";

                ExportToJsonData tExp = new ExportToJsonData(tfullname, tdata);
                tExp.StartExport();
            }
        }

        public void SaveFile()
        {
            List<string> tsheetnames = GetSheetNames();
            foreach (string tname in tsheetnames)
            {
                ExcelData tdata = GetContentHaveValue(tname);
                string tfullname = savepath + "/" + tname + ".bytes";

                ExportToData tExp = new ExportToData(tfullname, tdata);
                tExp.StartExport();
            }
        }


        public void ExoprtCfg(int starLine, StreamWriter logWriter, bool contentLog, bool isCrypt)
        {
            List<string> tsheetnames = GetSheetNames();
            foreach (string tname in tsheetnames)
            {
                Console.WriteLine("Export " + tname);
                ExcelData tdata = GetContentHaveValue(tname);
                string tfullname = savepath + "/" + tname + ".bytes";

                ExportToData tExp = new ExportToData(tfullname, tdata);
                tExp.StartExport();

            }
            Console.WriteLine("Complete.");
        }

        public List<string> ExportReadClass()
        {
            List<string> tsheetnames = GetSheetNames();
            foreach (string tname in tsheetnames)
            {
                ExcelData tdata = GetContentHaveValue(tname);
                string tfullname = savepath + "/" + tname + ".cs";

                ExportToCS tcs = new ExportToCS(tname, tfullname, tdata);
                tcs.StartExport();
            }

            return tsheetnames;
        }


    }

}
