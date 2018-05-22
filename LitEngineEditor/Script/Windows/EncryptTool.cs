﻿using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
namespace LitEngineEditor
{
    public class EncryptTool : ExportBase
    {
        private Vector2 mScrollPosition = Vector2.zero;
        private StringBuilder mContext = new StringBuilder();
        public EncryptTool(ExportWindow _window):base(_window)
        {
            ExWType = ExportWType.EncryptToolWindow;
            RestFileList();
        }

        override public void OnGUI()
        {
           // GUILayout.Label("EncryptKey", EditorStyles.boldLabel);
           // GUILayout.Label(ExportSetting.sEncryptKey);

            mScrollPosition = PublicGUI.DrawScrollview("Files",mContext.ToString(), mScrollPosition, mWindow.position.size.x, 150);

            GUILayout.Label("EncryptPath", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("", ExportSetting.sEncryptPath, EditorStyles.textField);
            if (GUILayout.Button("...", GUILayout.Width(25)))
            {
                string toldstr = ExportSetting.sEncryptPath;
                toldstr = EditorUtility.OpenFolderPanel("file Path", toldstr, "");
                if (!string.IsNullOrEmpty(toldstr) && !toldstr.Equals(ExportSetting.sEncryptPath))
                {
                    ExportSetting.sEncryptPath = toldstr;
                    NeedSaveSetting();

                    RestFileList();
                }

            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Encrypt all file"))
            {
                if (EditorUtility.DisplayDialog("Encrypt", " Start Encrypt?", "ok", "cancel"))
                {
                    string[] files = Directory.GetFiles(ExportSetting.sEncryptPath, "*.*", SearchOption.AllDirectories);
                    foreach(string filename in files)
                    {
                        LitEngine.IO.AesStreamBase.EnCryptFile(filename);
                    }
                    DLog.LogFormat("End of Encrype.count = {0}", files.Length);
                    UnityEditor.AssetDatabase.Refresh();
                }
            }

            if (GUILayout.Button("Decrypt all file"))
            {
                if (EditorUtility.DisplayDialog("Decrypt", " Start Decrypt?", "ok", "cancel"))
                {
                    string[] files = Directory.GetFiles(ExportSetting.sEncryptPath, "*.*", SearchOption.AllDirectories);
                    foreach (string filename in files)
                    {
                        LitEngine.IO.AesStreamBase.DeCryptFile(filename);
                    }
                    DLog.LogFormat("End of Decrype.count = {0}", files.Length);
                    UnityEditor.AssetDatabase.Refresh();
                }
            }

        }

        public void RestFileList()
        {
            if (!string.IsNullOrEmpty(ExportSetting.sEncryptPath) && Directory.Exists(ExportSetting.sEncryptPath))
            {
                mContext.Remove(0, mContext.Length);
                string[] files = Directory.GetFiles(ExportSetting.sEncryptPath, "*.*", SearchOption.AllDirectories);
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
                mContext.AppendLine( _text);
            }

        }

        public void AddSpace()
        {
            lock (mContext)
            {
                mContext.AppendLine();
            }

        }
    }
}
