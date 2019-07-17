using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using LitEngine.ScriptInterface;
using LitEngine;
using System.Text;
namespace LitEngineEditor
{
    public class ExportExcelWiindow : ExportBase
    {
        private Vector2 mScrollPosition = Vector2.zero;
        private StringBuilder mContext = new StringBuilder();
        public ExportExcelWiindow() : base()
        {
            ExWType = ExportWType.ExcelWindow;
        }
        override public void OnGUI()
        {
            mScrollPosition = PublicGUI.DrawScrollview("Files", mContext.ToString(), mScrollPosition, mWindow.position.size.x, 150);

            GUILayout.Label("ExcelPath", EditorStyles.boldLabel);

            //excel目录
            EditorGUILayout.BeginHorizontal();
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

            //导出c#目录
            EditorGUILayout.BeginHorizontal();
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

            if (GUILayout.Button("Export Bytes"))
            {
                if (EditorUtility.DisplayDialog("Export To Bytes", " Start Export?", "ok", "cancel"))
                {
                    string[] files = Directory.GetFiles(ExportSetting.Instance.sEncryptPath, "*.*", SearchOption.AllDirectories);
                    foreach (string filename in files)
                    {
                     
                    }
                    DLog.LogFormat("Complete  Export Data .count = {0}", files.Length);
                    UnityEditor.AssetDatabase.Refresh();
                }
            }

            if (GUILayout.Button("Export C#"))
            {
                if (EditorUtility.DisplayDialog("Export C#", " Start Export C#?", "ok", "cancel"))
                {
                    string[] files = Directory.GetFiles(ExportSetting.Instance.sEncryptPath, "*.*", SearchOption.AllDirectories);
                    foreach (string filename in files)
                    {
                        LitEngine.IO.AesStreamBase.DeCryptFile(filename);
                    }
                    DLog.LogFormat("Complete  Export C# .count = {0}", files.Length);
                    UnityEditor.AssetDatabase.Refresh();
                }
            }


        }

        public void RestFileList()
        {
            if (!string.IsNullOrEmpty(ExportSetting.Instance.sExcelPath) && Directory.Exists(ExportSetting.Instance.sExcelPath))
            {
                mContext.Remove(0, mContext.Length);
                string[] files = Directory.GetFiles(ExportSetting.Instance.sExcelPath, "*.*", SearchOption.AllDirectories);
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
    }
}
