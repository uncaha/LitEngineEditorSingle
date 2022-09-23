﻿using System.IO;
using System.Text;
using LitEngine.Tool;
using UnityEngine.WSA;

namespace LitEngineEditor
{
    public class ExportSetting
    {
        #region saved
        private const string sSavedCfg = "LEECfg.json";
        
        static private ExportSetting sIntance = null;

        static public ExportSetting Instance
        {
            get
            {
                if (sIntance == null)
                {
                    sIntance = new ExportSetting();
                    if (!File.Exists(filePath))
                    {
                        DataConvert.MergeFromJson(sIntance, File.ReadAllText(filePath));
                    }
                }

                return sIntance;
            }
        }

        public static string filePath => $"{ExportObject.GetFormatPath(System.IO.Directory.GetCurrentDirectory())}/{sSavedCfg}";

        public int sCompressed = 0;
        public int sBuildType = 0;
        public int sPathType = 1;
        public int sSelectedPlatm = 0;

        public string sMoveAssetsFilePath = "";
        //proto path
        public string sProtoFilePath = "";
        public string sCSFilePath = "";
        public string sProtoClassString = "";

        //encrypt
        public string sEncryptPath = "";
        //meshtool
        public string sMeshExportPath = "";

        //excel
        public string sExcelPath = "";
        public string sExcelBytesPath = "";
        public string sExcelSharpPath = "";
        #endregion
        static private void Rest()
        {
            sIntance = null;
        }

        private ExportSetting()
        {
            
        }
     
        static public void SaveCFG()
        {
            if (sIntance == null) return;
            DLog.Log("save:"+ filePath);
            string tjson = DataConvert.ToJson(sIntance);
            File.WriteAllText(filePath, tjson);

        }
    }
}
