using System;
using System.Collections.Generic;
using System.IO;
namespace ExportTool
{
    public class ExportConfigManager
    {
        string cfgMgrUp = @"
using System;
using System.Collections.Generic;
namespace Config{
    public abstract class ConfigBase { }
    public class ConfigManager{
        private static object lockobj = new object();
        private static ConfigManager sInstance = null;
        public static ConfigManager Instance{
            get{
                if (sInstance == null){
                    lock (lockobj){
                        if (sInstance == null){
                            sInstance = new ConfigManager();
                        }
                    }
                }
                return sInstance;
            }
        }
        private Dictionary<Type, ConfigBase> Dic = new Dictionary<Type, ConfigBase>();
        private ConfigManager() { ";
        string cfgMgrdown = @"
        }
        private void Add<T>() where T : ConfigBase, new(){
            T tcfg = new T();
            Dic.Add(typeof(T), tcfg);
        }

        public static T Get<T>() where T : ConfigBase{
            if (!Instance.Dic.ContainsKey(typeof(T))) return null;
            return (T)Instance.Dic[typeof(T)];
        }

    }
}
        ";

        List<string> configNames;
        string fullPathName;

        string tempFile;
        public ExportConfigManager(string pFullPath, List<string> pNamses)
        {
            fullPathName = pFullPath;
            configNames = pNamses;

            tempFile = fullPathName + ".temp";
        }

        public void StartExport()
        {
            try
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                using (FileStream tfile = File.OpenWrite(tempFile))
                {
                    ExportTool.TextWriter twt = new ExportTool.TextWriter(tfile);

                    twt.WriteLine(cfgMgrUp);
                    twt.Indent().Indent().Indent();
                    foreach (string tcfg in configNames)
                    {
                        twt.WriteLine($"Add<{tcfg}>();");
                    }
                    twt.Outdent().Outdent().Outdent();
                    twt.WriteLine(cfgMgrdown);
                    twt.Close();
                    tfile.Close();

                    if (File.Exists(fullPathName))
                        File.Delete(fullPathName);
                    File.Move(tempFile, fullPathName);
                }
            }
            catch (Exception pErro)
            {
               DLog.LogError(pErro.ToString());
            }

        }
    }
}
