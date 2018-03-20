using UnityEngine;
using UnityEditor;
using System.Text;
using System.Security;
using System.Collections;
using System.IO;
namespace LitEngineEditor
{
    public class ExportConfig 
    {
        #region datepath
        private const string cFileName = "AppConfig.xml";
        public string sDefaultFolder = "Assets/BundlesResources/Data/"; // 默认导出路径,统一不可更改
        public string sResourcesPath = "Assets/NeedExportResources/"; //需要导出的资源文件夹
        public string sEditorBundleFolder = "Assets/../Data/"; //编辑器工程外部资源路径
        public string sStreamingBundleFolder = "Assets/StreamingAssets/Data/";//内部资源路径
        public const string sResDataPath = "/ResData/";//app资源路径,固定分级
        #endregion

        
        public static string GetTartFolder(BuildTarget _target)
        {
            return string.Format("/{0}/", _target.ToString());
        }

        public ExportConfig()
        {
            
        }

        public void LoadConfig()
        {
            string tfullpath = System.IO.Directory.GetCurrentDirectory() + "\\Assets\\Editor\\" + cFileName;
            tfullpath = tfullpath.Replace("\\", "/");
            LitEngine.XmlLoad.SecurityParser txmlfile = new LitEngine.XmlLoad.SecurityParser();
            txmlfile.LoadXml(System.IO.File.ReadAllText(tfullpath));
            SecurityElement txmlroot = txmlfile.ToXml();
            SecurityElement tapp = txmlroot.SearchForChildByTag("App");
            sDefaultFolder = tapp.SearchForChildByTag("DefaultExportPath").Text;
            sResourcesPath = tapp.SearchForChildByTag("NeedExportPath").Text;
            sEditorBundleFolder = tapp.SearchForChildByTag("EditorBundlePath").Text;
            sStreamingBundleFolder = tapp.SearchForChildByTag("StreamingBundlePath").Text;
        }


        public void OnGUI()
        {
            GUILayout.Label("Config:", EditorStyles.boldLabel);
            StringBuilder tbuilder = new StringBuilder();
            tbuilder.AppendLine(string.Format("[Platm]:{0}", ExportObject.sPlatformList[ExportSetting.sSelectedPlatm]));
            tbuilder.AppendLine(string.Format("[Compressed]:{0}", ExportSetting.sCompressed == 0 ? true : false));
            tbuilder.AppendLine(string.Format("[ResourcesPath]:{0}", sResourcesPath));
            tbuilder.AppendLine(string.Format("[ExportPath]:{0}/{1}", sDefaultFolder, GetTartFolder(ExportObject.sBuildTarget[ExportSetting.sSelectedPlatm])).Replace("//", "/"));
            tbuilder.AppendLine(string.Format("[SidePath]:{0}/{1}", sEditorBundleFolder, sResDataPath).Replace("//", "/"));
            tbuilder.AppendLine(string.Format("[StreamingPath]:{0}/{1}", sStreamingBundleFolder,sResDataPath).Replace("//","/"));

            GUILayout.Box(tbuilder.ToString(), EditorStyles.textField);
        }
    }
}
