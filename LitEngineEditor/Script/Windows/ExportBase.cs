using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitEngineEditor
{
    public enum ExportWType
    {
        AssetsWindow = 0,
        PrptoWindow,
        EncryptToolWindow,
    }
    public abstract class ExportBase
    {
        protected ExportWindow mWindow;
        private static ExportConfig sConfig = null;
        public static ExportConfig Config
        {
            get
            {
                if (sConfig == null)
                {
                    sConfig = new ExportConfig();
                    sConfig.LoadConfig();
                }
                    
                return sConfig;
            }
        }
        public ExportWType ExWType { get; protected set; }
        public static void RestConfig()
        {
            Config.LoadConfig();
        }
        private ExportBase()
        {
            
        }

        public ExportBase(ExportWindow _windows)
        {
            mWindow = _windows;
        }

        public void NeedSaveSetting()
        {
            mWindow.NeedSaveSetting = true;
        }

        abstract public void OnGUI();
    }
}
