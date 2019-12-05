using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using LitEngine.ScriptInterface;
using LitEngine;
using System.Text;
using ExportTool;
namespace LitEngineEditor
{
    public class ExportExcelWiindow : ExportBase
    {
        private Vector2 mScrollPosition = Vector2.zero;
        private StringBuilder mContext = new StringBuilder();
        protected string filestag = "*.xls";
        public ExportExcelWiindow() : base()
        {
            ExWType = ExportWType.ExcelWindow;
            RestFileList();
        }
        override public void OnGUI()
        {
            mScrollPosition = PublicGUI.DrawScrollview("Files", mContext.ToString(), mScrollPosition, mWindow.position.size.x, 160);

            
            //excel目录
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Ex:", GUILayout.Width(35));
            EditorGUILayout.TextField("", ExportSetting.Instance.sExcelPath, EditorStyles.textField);
            if (GUILayout.Button("...", GUILayout.Width(25)))
            {
                string toldstr = ExportSetting.Instance.sExcelPath;
                toldstr = EditorUtility.OpenFolderPanel("file Path", toldstr, "");
                if (!string.IsNullOrEmpty(toldstr) && !toldstr.Equals(ExportSetting.Instance.sExcelPath))
                {
                    ExportSetting.Instance.sExcelPath = toldstr;
                    NeedSaveSetting();

                    RestFileList();
                }

            }
            EditorGUILayout.EndHorizontal();

            //导出byte目录
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("DB:", GUILayout.Width(35));
            EditorGUILayout.TextField("", ExportSetting.Instance.sExcelBytesPath, EditorStyles.textField);
            if (GUILayout.Button("...", GUILayout.Width(25)))
            {
                string toldstr = ExportSetting.Instance.sExcelBytesPath;
                toldstr = EditorUtility.OpenFolderPanel("file Path", toldstr, "");
                if (!string.IsNullOrEmpty(toldstr) && !toldstr.Equals(ExportSetting.Instance.sExcelBytesPath))
                {
                    ExportSetting.Instance.sExcelBytesPath = toldstr;
                    NeedSaveSetting();
                }

            }
            EditorGUILayout.EndHorizontal();

            //导出c#目录
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("C#:", GUILayout.Width(35));
            EditorGUILayout.TextField("", ExportSetting.Instance.sExcelSharpPath, EditorStyles.textField);
            if (GUILayout.Button("...", GUILayout.Width(25)))
            {
                string toldstr = ExportSetting.Instance.sExcelSharpPath;
                toldstr = EditorUtility.OpenFolderPanel("file Path", toldstr, "");
                if (!string.IsNullOrEmpty(toldstr) && !toldstr.Equals(ExportSetting.Instance.sExcelSharpPath))
                {
                    ExportSetting.Instance.sExcelSharpPath = toldstr;
                    NeedSaveSetting();
                }

            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Export Bytes"))
            {
                if (string.IsNullOrEmpty(ExportSetting.Instance.sExcelBytesPath)) return;
                if (EditorUtility.DisplayDialog("Export To Bytes", " Start Export?", "ok", "cancel"))
                {
                    string[] files = Directory.GetFiles(ExportSetting.Instance.sExcelPath, filestag, SearchOption.AllDirectories);
                    foreach (string filename in files)
                    {
                        ExcelClass texcel = new ExcelClass(filename, ExportSetting.Instance.sExcelBytesPath);
                        texcel.SaveFile();
                        texcel.Close();
                    }
                    DLog.LogFormat("Complete  Export Data .count = {0}", files.Length);
                    UnityEditor.AssetDatabase.Refresh();
                }
            }

            if (GUILayout.Button("Export C#"))
            {
                if (string.IsNullOrEmpty(ExportSetting.Instance.sExcelSharpPath)) return;
                WriteShapFile();
            }
        }

        public void RestFileList()
        {
            if (!string.IsNullOrEmpty(ExportSetting.Instance.sExcelPath) && Directory.Exists(ExportSetting.Instance.sExcelPath))
            {
                mContext.Remove(0, mContext.Length);
                string[] files = Directory.GetFiles(ExportSetting.Instance.sExcelPath,filestag, SearchOption.AllDirectories);
                foreach (string filename in files)
                {
                    AddContext(filename);
                }
            }
        }

        public void AddContext(string _text)
        {
            lock (mContext)
            {
                mContext.AppendLine(_text);
            }

        }
        private void WriteShapFile()
        {
            if (EditorUtility.DisplayDialog("Export C#", " Start Export C#?", "ok", "cancel"))
            {
                string[] files = Directory.GetFiles(ExportSetting.Instance.sExcelPath, filestag, SearchOption.AllDirectories);
                List<string> tcfgs = new List<string>();
                foreach (string filename in files)
                {
                    ExcelClass texcel = new ExcelClass(filename, ExportSetting.Instance.sExcelSharpPath);
                    List<string> tsnames = texcel.ExportReadClass();
                    texcel.Close();
                    tcfgs.AddRange(tsnames);
                }

                ExportConfigManager tem = new ExportConfigManager(ExportSetting.Instance.sExcelSharpPath + "/ConfigManager.cs", tcfgs);
                tem.StartExport();

                DLog.LogFormat("Complete  Export C# .count = {0}", files.Length);
                UnityEditor.AssetDatabase.Refresh();
            }
        }
    }

}
