﻿using UnityEngine;
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
        public static string[] sBuildType = new string[4] { "every", "depInOne" ,"allInOne","folder"};
        private static readonly BuildAssetBundleOptions[] sBuildOption = { BuildAssetBundleOptions.ChunkBasedCompression, BuildAssetBundleOptions.UncompressedAssetBundle };
        public ExportObject() : base()
        {
            ExWType = ExportWType.AssetsWindow;
        }

        override public void OnGUI()
        {
            GUILayout.Label("Platform", EditorStyles.boldLabel);
            int oldSelectedPlatm = ExportSetting.Instance.sSelectedPlatm;
            int oldcompressed = ExportSetting.Instance.sCompressed;
            ExportSetting.Instance.sSelectedPlatm = GUILayout.SelectionGrid(ExportSetting.Instance.sSelectedPlatm, sPlatformList, 3);
            ExportSetting.Instance.sCompressed = GUILayout.SelectionGrid(ExportSetting.Instance.sCompressed, sCompressed, 2);
            ExportSetting.Instance.sBuildType = GUILayout.SelectionGrid(ExportSetting.Instance.sBuildType, sBuildType, 4);

            if (oldSelectedPlatm != ExportSetting.Instance.sSelectedPlatm || oldcompressed != ExportSetting.Instance.sCompressed)
                NeedSaveSetting();

            Config.OnGUI();

            if (GUILayout.Button("Export Assets"))
            {
                ExportAllBundle(sBuildTarget[ExportSetting.Instance.sSelectedPlatm]);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Move Assets to SidePath"))
            {
                MoveBundleToSideDate(sBuildTarget[ExportSetting.Instance.sSelectedPlatm]);
            }
            if (GUILayout.Button("Move Assets to StreamPath"))
            {
                MoveBUndleToStreamingPath(sBuildTarget[ExportSetting.Instance.sSelectedPlatm]);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Move to"))
            {
                string toldpath = ExportSetting.Instance.sMoveAssetsFilePath;
                toldpath = EditorUtility.OpenFolderPanel("Move to path", toldpath, "");
                if (!string.IsNullOrEmpty(toldpath) && !toldpath.Equals(ExportSetting.Instance.sMoveAssetsFilePath))
                {
                    ExportSetting.Instance.sMoveAssetsFilePath = toldpath;
                    NeedSaveSetting();
                }
                if (string.IsNullOrEmpty(ExportSetting.Instance.sMoveAssetsFilePath)) return;
                Config.LoadConfig();
                BuildTarget _target = sBuildTarget[ExportSetting.Instance.sSelectedPlatm];
                string tpath = Config.sDefaultFolder + ExportConfig.GetTartFolder(_target);
                MoveToPath(tpath, ExportSetting.Instance.sMoveAssetsFilePath, ExportConfig.GetTartFolder(_target));
            }
        }

        #region export

        public static void ExportAllBundle(BuildTarget _target)
        {
            string tpath = Config.sDefaultFolder + ExportConfig.GetTartFolder(_target);

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

            DirectoryInfo tdirfolder = new DirectoryInfo(Config.sResourcesPath);
            FileInfo[] tfileinfos = tdirfolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            bool tisCanExport = false;
            //  Dictionary<string, ExObject> tfiledic = new Dictionary<string, ExObject>();
            Dictionary<string, List<string>> tHavedfiledic = new Dictionary<string, List<string>>();
            Dictionary<string, string> tfiledic = new Dictionary<string, string>();
            foreach (FileInfo tfile in tfileinfos)
            {
                if (tfile.Name.EndsWith(".meta")) continue;
                if (tfiledic.ContainsKey(tfile.Name))
                {
                    if (!tHavedfiledic.ContainsKey(tfile.Name))
                        tHavedfiledic.Add(tfile.Name, new List<string>());

                    if (!tHavedfiledic[tfile.Name].Contains(tfiledic[tfile.Name]))
                        tHavedfiledic[tfile.Name].Add(tfiledic[tfile.Name]);

                    if (!tHavedfiledic[tfile.Name].Contains(tfile.FullName))
                        tHavedfiledic[tfile.Name].Add(tfile.FullName);

                    tisCanExport = true;
                    continue;
                }
                else
                {
                    tfiledic.Add(tfile.Name, tfile.FullName);
                }

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
            {
                GoExport(tpath, builds.ToArray(), _target);
            }
            else
            {
                DLog.LogError("存在重名文件.导出失败.");

                List<string> tkeys = new List<string>(tHavedfiledic.Keys);
                foreach (var key in tkeys)
                {
                    System.Text.StringBuilder tstrbuilder = new System.Text.StringBuilder();
                    tstrbuilder.AppendLine("重名文件:" + key);
                    tstrbuilder.AppendLine("{");
                    List<string> tfiles = tHavedfiledic[key];
                    foreach (var item in tfiles)
                    {
                        tstrbuilder.AppendLine("    " + item);
                    }

                    tstrbuilder.AppendLine("}");
                    DLog.LogError(tstrbuilder.ToString());
                }

            }

        }

        static Dictionary<string, UnityEditor.AssetBundleBuild> waitExportFiles = new Dictionary<string, UnityEditor.AssetBundleBuild>();

        public static void ExportAllBundleFullPath(BuildTarget _target)
        {
            waitExportFiles.Clear();
            string tpath = Config.sDefaultFolder + ExportConfig.GetTartFolder(_target);

            List<UnityEditor.AssetBundleBuild> builds = null;
            switch (ExportSetting.Instance.sBuildType)
            {
                case 0:
                case 1:
                    builds = GetBunldeBuildsEvery(tpath);
                    break;
                case 2:
                    builds = GetBunldeBuildsAllInOne(tpath);
                    break;
                case 3:
                    builds = GetBunldeBuildsFolder(tpath);
                    break;
            }
            GoExport(tpath, builds.ToArray(), _target);
        }

        static List<UnityEditor.AssetBundleBuild> GetBunldeBuildsFolder(string path)
        {
            waitExportFiles.Clear();
            string tpath = path;
            List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>();
            DirectoryInfo tdirfolder = new DirectoryInfo(Config.sResourcesPath);
            DirectoryInfo[] dirs = tdirfolder.GetDirectories("*.*", System.IO.SearchOption.TopDirectoryOnly);

            foreach (var curDirectory in dirs)
            {
                FileInfo[] tfileinfos = curDirectory.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

                UnityEditor.AssetBundleBuild tbuild = new UnityEditor.AssetBundleBuild();
                tbuild.assetBundleName = curDirectory.Name + LitEngine.Loader.BaseBundle.sSuffixName;

                List<string> tnamelist = new List<string>();
                foreach (FileInfo tfile in tfileinfos)
                {
                    if (tfile.Name.EndsWith(".meta")) continue;
                    if (tfile.Name.EndsWith(".cs") || tfile.Name.EndsWith(".dll")) continue;
                    string tresPath = GetResPath(tfile.FullName);
                    tnamelist.Add(tresPath);
                }

                tbuild.assetNames = tnamelist.ToArray();
                builds.Add(tbuild);
            }

            return builds;
        }

        static List<UnityEditor.AssetBundleBuild> GetBunldeBuildsAllInOne(string path)
        {
            waitExportFiles.Clear();
            string tpath = path;
            List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>();

            DirectoryInfo tdirfolder = new DirectoryInfo(Config.sResourcesPath);
            FileInfo[] tfileinfos = tdirfolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            UnityEditor.AssetBundleBuild tbuild = new UnityEditor.AssetBundleBuild();
            tbuild.assetBundleName = "allinone" + LitEngine.Loader.BaseBundle.sSuffixName;

            List<string> tnamelist = new List<string>();
            foreach (FileInfo tfile in tfileinfos)
            {
                if (tfile.Name.EndsWith(".meta")) continue;
                if (tfile.Name.EndsWith(".cs") || tfile.Name.EndsWith(".dll")) continue;
                string tresPath = GetResPath(tfile.FullName);
                tnamelist.Add(tresPath);
            }

            tbuild.assetNames = tnamelist.ToArray();
            builds.Add(tbuild);
            return builds;
        }

        static List<UnityEditor.AssetBundleBuild> GetBunldeBuildsEvery(string path)
        {
            waitExportFiles.Clear();
            string tpath = path;

            List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>();

            DirectoryInfo tdirfolder = new DirectoryInfo(Config.sResourcesPath);
            FileInfo[] tfileinfos = tdirfolder.GetFiles("*.*", System.IO.SearchOption.AllDirectories);


            foreach (FileInfo tfile in tfileinfos)
            {
                if (tfile.Name.EndsWith(".meta")) continue;
                if (tfile.Name.EndsWith(".cs") || tfile.Name.EndsWith(".dll")) continue;
                string tresPath = GetResPath(tfile.FullName);
                if (waitExportFiles.ContainsKey(tresPath))
                {
                    continue;
                }

                UnityEditor.AssetBundleBuild tbuild = GetAssetBundleBuild(tfile.FullName);
                builds.Add(tbuild);

                waitExportFiles.Add(tresPath, tbuild);
                if (ExportSetting.Instance.sBuildType == 0)
                {
                    AddAssetFromDependencles(tresPath, ref builds);
                }
            }

            return builds;
        }

        static string GetResPath(string pFullPath)
        {
            string tresPath = pFullPath;
            int tindex = tresPath.IndexOf("Assets");
            tresPath = tresPath.Substring(tindex, tresPath.Length - tindex);
            tresPath = tresPath.Replace("\\", "/");
            return tresPath;
        }

        static void AddAssetFromDependencles(string presPath, ref List<UnityEditor.AssetBundleBuild> buildList)
        {
            string[] tresdepends = AssetDatabase.GetDependencies(presPath, true);
            foreach (var item in tresdepends)
            {
                if (item.EndsWith(".cs") || item.EndsWith(".dll")) continue;

                if (waitExportFiles.ContainsKey(item)) continue;

                UnityEditor.AssetBundleBuild tbuild = GetAssetBundleBuild(item);
                buildList.Add(tbuild);

                waitExportFiles.Add(item, tbuild);
                AddAssetFromDependencles(item, ref buildList);
            }
        }
        static UnityEditor.AssetBundleBuild GetAssetBundleBuild(string pFileName)
        {
            string tresPath = GetResPath(pFileName);
            UnityEditor.AssetBundleBuild tbuild = new UnityEditor.AssetBundleBuild();
            tbuild.assetBundleName = tresPath + LitEngine.Loader.BaseBundle.sSuffixName;
            tbuild.assetNames = new string[] { tresPath };
            return tbuild;
        }

        public static void GoExport(string _path, AssetBundleBuild[] _builds, BuildTarget _target)
        {
            if(_builds == null) return;
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            if (_builds.Length == 0) return;
            BuildPipeline.BuildAssetBundles(_path, _builds, sBuildOption[ExportSetting.Instance.sCompressed] | BuildAssetBundleOptions.DeterministicAssetBundle, _target);

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
            string tpath = Config.sDefaultFolder + ExportConfig.GetTartFolder(_target);
            string tfullpath = System.IO.Directory.GetCurrentDirectory() + "\\" + Config.sEditorBundleFolder + ExportConfig.sResDataPath;
            tfullpath = tfullpath.Replace("\\", "/");
            MoveToPath(tpath, tfullpath, ExportConfig.GetTartFolder(_target));
        }

        public static void MoveToPath(string _socPath, string _desPath, string _targetname)
        {
            if (!Directory.Exists(_desPath))
                Directory.CreateDirectory(_desPath);
            DeleteAllFile(_desPath);
            _socPath = _socPath.Replace("//", "/");
            DirectoryInfo tdirfolder = new DirectoryInfo(_socPath);

            FileInfo[] tfileinfos = tdirfolder.GetFiles("*" + LitEngine.Loader.BaseBundle.sSuffixName, System.IO.SearchOption.AllDirectories);

            foreach (FileInfo tfile in tfileinfos)
            {
                string tresPath = tfile.FullName.Replace("//", "/");
                int tindex = tresPath.IndexOf(_socPath) + _socPath.Length;
                tresPath = tresPath.Substring(tindex, tresPath.Length - tindex);


                string dicPath = (_desPath + "/" + tresPath.Replace(tfile.Name, "")).Replace("//", "/");

                if (!Directory.Exists(dicPath))
                    Directory.CreateDirectory(dicPath);

                File.Copy(tfile.FullName, _desPath + "/" + tresPath, true);
            }

            Debug.Log("移动完成.");
        }

        static void DeleteAllFile(string _path, string searchPatter = "*.*")
        {
            if (!Directory.Exists(_path)) return;

            string[] tfiles = Directory.GetFiles(_path, searchPatter, System.IO.SearchOption.AllDirectories);
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
