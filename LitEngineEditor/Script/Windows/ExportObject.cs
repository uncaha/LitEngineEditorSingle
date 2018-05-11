using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using LitEngine.ScriptInterface;
using LitEngine;
namespace LitEngineEditor
{
    public class ExportObject : ExportBase
    {
        public static readonly string[] sPlatformList = new string[] { "Android", "iOS", "Windows64" };
        public static readonly BuildTarget[] sBuildTarget = { BuildTarget.Android, BuildTarget.iOS, BuildTarget.StandaloneWindows64 };

        public static string[] sCompressed = new string[2] { "Compressed", "UnCompressed" };
        private static readonly BuildAssetBundleOptions[] sBuildOption= { BuildAssetBundleOptions.ChunkBasedCompression, BuildAssetBundleOptions .UncompressedAssetBundle};
        public ExportObject(ExportWindow _window) : base(_window)
        {
            ExWType = ExportWType.AssetsWindow;
        }

        override public void OnGUI()
        {
            GUILayout.Label("Platform", EditorStyles.boldLabel);
            int oldSelectedPlatm = ExportSetting.sSelectedPlatm;
            int oldcompressed = ExportSetting.sCompressed;
            ExportSetting.sSelectedPlatm = GUILayout.SelectionGrid(ExportSetting.sSelectedPlatm, sPlatformList, 3);
            ExportSetting.sCompressed = GUILayout.SelectionGrid(ExportSetting.sCompressed, sCompressed, 2);

            if(oldSelectedPlatm != ExportSetting.sSelectedPlatm || oldcompressed != ExportSetting.sCompressed)
                NeedSaveSetting();

            Config.OnGUI();

            if (GUILayout.Button("Export Assets"))
            {
                ExportAllBundle(sBuildTarget[ExportSetting.sSelectedPlatm]);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Move Assets to SidePath"))
            {
                MoveBundleToSideDate(sBuildTarget[ExportSetting.sSelectedPlatm]);
            }
            if (GUILayout.Button("Move Assets to StreamPath"))
            {
                MoveBUndleToStreamingPath(sBuildTarget[ExportSetting.sSelectedPlatm]);
            }
            EditorGUILayout.EndHorizontal();


        }

        #region export
        public static void ExportAllBundle(BuildTarget _target)
        {
            string tpath = Config.sDefaultFolder + ExportConfig.GetTartFolder(_target);

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

            DirectoryInfo tdirfolder = new DirectoryInfo(Config.sResourcesPath);
            FileInfo[] tfileinfos = tdirfolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            foreach (FileInfo tfile in tfileinfos)
            {
                if (tfile.Name.EndsWith(".meta")) continue;
                AssetBundleBuild tbuild = new AssetBundleBuild();
                tbuild.assetBundleName = tfile.Name + LitEngine.Loader.BaseBundle.sSuffixName;
                string tRelativePath = tfile.FullName;
                int tindex = tRelativePath.IndexOf("Assets");
                tRelativePath = tRelativePath.Substring(tindex, tRelativePath.Length - tindex);
                tRelativePath = tRelativePath.Replace("\\", "/");
                tbuild.assetNames = new string[] { tRelativePath };
                builds.Add(tbuild);
            }

            GoExport(tpath, builds.ToArray(), _target);
        }

        public static void GoExport(string _path, AssetBundleBuild[] _builds, BuildTarget _target)
        {
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            if (_builds.Length == 0) return;
            BuildPipeline.BuildAssetBundles(_path, _builds, sBuildOption[ExportSetting.sCompressed], _target);

            string tmanifestname = ExportConfig.GetTartFolder(_target).Replace("/", "");
            string tdespathname = _path + "/" + LoaderManager.ManifestName + LitEngine.Loader.BaseBundle.sSuffixName;
            if (File.Exists(tdespathname))
                File.Delete(tdespathname);
            File.Copy(_path + tmanifestname, tdespathname);
           // AssetDatabase.Refresh();
            Debug.Log("导出完成!");
        }
        #endregion

        #region move

        static void MoveBUndleToStreamingPath(BuildTarget _target)
        {
            Config.LoadConfig();
            string tpath = Config.sDefaultFolder + ExportConfig.GetTartFolder(_target);
            string tfullpath = System.IO.Directory.GetCurrentDirectory() + "\\" + Config.sStreamingBundleFolder + ExportConfig.sResDataPath;
            tfullpath = tfullpath.Replace("\\", "/");
            MoveToPath(tpath, tfullpath, ExportConfig.GetTartFolder(_target));
            AssetDatabase.Refresh();
        }

        static void MoveBundleToSideDate(BuildTarget _target)
        {
            Config.LoadConfig();
            string tpath = Config.sDefaultFolder  + ExportConfig.GetTartFolder(_target);
            string tfullpath = System.IO.Directory.GetCurrentDirectory() + "\\" + Config.sEditorBundleFolder  + ExportConfig.sResDataPath;
            tfullpath = tfullpath.Replace("\\", "/");
            MoveToPath(tpath, tfullpath, ExportConfig.GetTartFolder(_target));
        }

        public static void MoveToPath(string _socPath, string _desPath, string _targetname)
        {
            if (!Directory.Exists(_desPath))
                Directory.CreateDirectory(_desPath);

            DirectoryInfo tdirfolder = new DirectoryInfo(_socPath);

            FileInfo[] tfileinfos = tdirfolder.GetFiles("*" + LitEngine.Loader.BaseBundle.sSuffixName, System.IO.SearchOption.AllDirectories);

            foreach (FileInfo tfile in tfileinfos)
            {
                File.Copy(tfile.FullName, _desPath + "/" + tfile.Name,true);
            }

            Debug.Log("移动完成.");
        }

        static void DeleteAllFile(string _path)
        {
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            DirectoryInfo tdirfolder = new DirectoryInfo(_path);
            FileInfo[] tfileinfos = tdirfolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            foreach (FileInfo tfile in tfileinfos)
                File.Delete(tfile.FullName);
            Debug.Log("清除目录结束.");
        }
        #endregion



    }
}
