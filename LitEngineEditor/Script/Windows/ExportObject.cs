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

            if (GUILayout.Button("Delete *.meta"))
            {
                DeleteMetaFile();
            }

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

            if (GUILayout.Button("Move tO"))
            {
                string toldpath = ExportSetting.sMoveAssetsFilePath;
                toldpath = EditorUtility.OpenFolderPanel("Move to path", toldpath, "");
                if (!string.IsNullOrEmpty(toldpath) && !toldpath.Equals(ExportSetting.sMoveAssetsFilePath))
                {
                    ExportSetting.sMoveAssetsFilePath = toldpath;
                    NeedSaveSetting();
                }
                if (string.IsNullOrEmpty(ExportSetting.sMoveAssetsFilePath)) return;
                Config.LoadConfig();
                BuildTarget _target = sBuildTarget[ExportSetting.sSelectedPlatm];
                string tpath = Config.sDefaultFolder + ExportConfig.GetTartFolder(_target);
                MoveToPath(tpath, ExportSetting.sMoveAssetsFilePath, ExportConfig.GetTartFolder(_target));
            }
        }

        #region export
        public static void DeleteMetaFile()
        {
            DeleteAllFile(Config.sResourcesPath, "*.meta");
            AssetDatabase.Refresh();
        }

        public static void ExportAllBundle(BuildTarget _target)
        {
            string tpath = Config.sDefaultFolder + ExportConfig.GetTartFolder(_target);

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

            DirectoryInfo tdirfolder = new DirectoryInfo(Config.sResourcesPath);
            FileInfo[] tfileinfos = tdirfolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            bool tisCanExport = false;
            Dictionary<string, string> tfiledic = new Dictionary<string, string>();
            foreach (FileInfo tfile in tfileinfos)
            {
                if (tfile.Name.EndsWith(".meta")) continue;
                if(tfiledic.ContainsKey(tfile.Name))
                {
                    DLog.LogErrorFormat("重名的文件.name1 = {0} \n name2 = {1}", tfiledic[tfile.Name],tfile.FullName);
                    tisCanExport = true;
                    continue;
                }

                tfiledic.Add(tfile.Name, tfile.FullName);

                AssetBundleBuild tbuild = new AssetBundleBuild();
                tbuild.assetBundleName = tfile.Name + LitEngine.Loader.BaseBundle.sSuffixName;
                string tRelativePath = tfile.FullName;
                int tindex = tRelativePath.IndexOf("Assets");
                tRelativePath = tRelativePath.Substring(tindex, tRelativePath.Length - tindex);
                tRelativePath = tRelativePath.Replace("\\", "/");
                tbuild.assetNames = new string[] { tRelativePath };
                builds.Add(tbuild);
            }
            if (!tisCanExport)
                GoExport(tpath, builds.ToArray(), _target);
            else
                DLog.LogError("导出失败.");
        }

        public static void GoExport(string _path, AssetBundleBuild[] _builds, BuildTarget _target)
        {
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            if (_builds.Length == 0) return;
            BuildPipeline.BuildAssetBundles(_path, _builds, sBuildOption[ExportSetting.sCompressed] | BuildAssetBundleOptions.DeterministicAssetBundle, _target);

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
            DeleteAllFile(_desPath);

            DirectoryInfo tdirfolder = new DirectoryInfo(_socPath);

            FileInfo[] tfileinfos = tdirfolder.GetFiles("*" + LitEngine.Loader.BaseBundle.sSuffixName, System.IO.SearchOption.AllDirectories);

            foreach (FileInfo tfile in tfileinfos)
            {
                File.Copy(tfile.FullName, _desPath + "/" + tfile.Name,true);
            }

            Debug.Log("移动完成.");
        }

        static void DeleteAllFile(string _path,string searchPatter = "*.*")
        {
            if (!Directory.Exists(_path)) return;

            string[] tfiles =  Directory.GetFiles(_path, searchPatter, System.IO.SearchOption.AllDirectories);
            for (int i = 0; i < tfiles.Length; i++)
            {
                string tfilename = tfiles[i];
                if (File.Exists(tfilename))
                {
                    FileInfo fi = new FileInfo(tfilename);
                    fi.Attributes = FileAttributes.Normal;
                    fi.Delete();
                }
            }
            Debug.Log("清除结束.");
        }
        #endregion



    }
}
