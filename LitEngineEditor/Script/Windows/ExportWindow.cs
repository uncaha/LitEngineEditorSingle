using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace LitEngineEditor
{
    public class ExportWindow : EditorWindow
    {
        #region Windows
        [UnityEditor.MenuItem("Export/ExportTool")]
        static void Init()
        {
            ExportWindow window = (ExportWindow)EditorWindow.GetWindow(typeof(ExportWindow));
            window.minSize = new Vector2(400, 330);
            window.maxSize = new Vector2(500, 330);
            window.name = "ExportTool";
            window.Show();
        }
        #endregion
        #region class
        #region field
        public bool NeedSaveSetting { get; set; }
        private int mToolbarOption = 0;
        private string[] mToolbarTexts = { "Assets", "ProtoToCS", "EnCryptTool" ,"MeshTool"};
        private Dictionary<ExportWType, ExportBase> mMap = new Dictionary<ExportWType, ExportBase>();
        #endregion

        public ExportWindow()
        {
            InitGUI();
        }

        void InitGUI()
        {
            ExportSetting.LoadCFG();
            ExportBase.RestConfig();
            ExportObject tassetobj = new ExportObject(this);
            mMap.Add(tassetobj.ExWType, tassetobj);

            ExportProtoTool tprototool = new ExportProtoTool(this);
            mMap.Add(tprototool.ExWType, tprototool);

            EncryptTool tencrypt = new EncryptTool(this);
            mMap.Add(tencrypt.ExWType, tencrypt);

            MeshTool tmeshtool = new MeshTool(this);
            mMap.Add(tmeshtool.ExWType, tmeshtool);
        }

        void UpdateGUI()
        {
            if (mMap.ContainsKey((ExportWType)mToolbarOption))
                mMap[(ExportWType)mToolbarOption].OnGUI();
        }

        void RestGUI()
        {
            mMap.Clear();
            InitGUI();
        }
        
        void OnGUI()
        {
            NeedSaveSetting = false;
            if (GUILayout.Button("Rest Config"))
            {
                RestGUI();
            }
            mToolbarOption = GUILayout.Toolbar(mToolbarOption, mToolbarTexts);
  
            UpdateGUI();

            if(NeedSaveSetting)
                ExportSetting.SaveCFG();
        }
        #endregion


    }
}

