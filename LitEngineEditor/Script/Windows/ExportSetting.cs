using System.IO;
using System.Text;
namespace LitEngineEditor
{
    public class ExportSetting
    {
        #region saved
        private const string sSavedCfg = "ExCfg.txt";
        static public int sCompressed = 1;
        static public int sSelectedPlatm = 0;
        //proto path
        static public string sProtoFilePath = "";
        static public string sCSFilePath = "";

        //encrypt
        static public string sEncryptPath = "";
        #endregion
        static private void Rest()
        {
            sCompressed = 1;
            sSelectedPlatm = 0;
            sProtoFilePath = "";
            sCSFilePath = "";
            sEncryptPath = "";
        }
        static public void LoadCFG()
        {
            Rest();
            string tfullpath = System.IO.Directory.GetCurrentDirectory() + "\\Assets\\Editor\\" + sSavedCfg;
            if (!File.Exists(tfullpath)) return;
            string[] tlines = File.ReadAllLines(tfullpath);
            foreach (string curline in tlines)
            {
                string[] spstrs = curline.Split('=');
                SetField(spstrs[0].Trim(), spstrs[1].Trim());
            }

        }

        static private void SetPath(ref string _source,string _value)
        {
            if (Directory.Exists(_value))
                _source = _value;
            else
                _source = "";
        }

        static private void SetField(string _key, string _value)
        {
            switch (_key)
            {
                case "sSelectedPlatm":
                    sSelectedPlatm = int.Parse(_value);
                    break;
                case "sCompressed":
                    sCompressed = int.Parse(_value);
                    break;
                case "sProtoFilePath":
                    SetPath(ref sProtoFilePath,_value);
                    break;
                case "sCSFilePath":
                    SetPath(ref sCSFilePath, _value);
                    break;
                case "sEncryptPath":
                    SetPath(ref sEncryptPath, _value);
                    break;
            }
        }

        static public void SaveCFG()
        {
            string tfullpath = System.IO.Directory.GetCurrentDirectory() + "\\Assets\\Editor\\" + sSavedCfg;
            StringBuilder tbuilder = new StringBuilder();
            tbuilder.AppendLine("sSelectedPlatm = " + sSelectedPlatm);
            tbuilder.AppendLine("sCompressed = " + sCompressed);
            tbuilder.AppendLine("sProtoFilePath = " + sProtoFilePath);
            tbuilder.AppendLine("sCSFilePath = " + sCSFilePath);
            tbuilder.AppendLine("sEncryptPath = " + sEncryptPath);

            File.WriteAllText(tfullpath, tbuilder.ToString());

        }
    }
}
