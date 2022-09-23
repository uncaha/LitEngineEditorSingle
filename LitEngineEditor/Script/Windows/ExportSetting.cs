using System.IO;
using System.Text;
using LitEngine.Tool;

namespace LitEngineEditor
{
    public class ExportSetting
    {
        #region saved
        private const string sSavedCfg = "ExCfg.json";
        private static string filePath = System.IO.Directory.GetCurrentDirectory() + "\\Assets\\Editor\\";
        static private ExportSetting sIntance = null;
        static public ExportSetting Instance
        {
            get
            {
                if (sIntance == null)
                {
                    string tfullpath = filePath + sSavedCfg;
                    if (!File.Exists(tfullpath))
                    {
                        sIntance = new ExportSetting();
                    }
                    else
                    {
                        sIntance = DataConvert.FromJson<ExportSetting>(File.ReadAllText(tfullpath));
                    }

                }
                    
                return sIntance;
            }
        }

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
            string tfullpath = filePath + sSavedCfg;
            DLog.Log("save:"+ tfullpath);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            string tjson = DataConvert.ToJson(sIntance);
            File.WriteAllText(tfullpath, tjson);

        }
    }
}
