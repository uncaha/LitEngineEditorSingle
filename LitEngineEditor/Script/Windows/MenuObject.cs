using UnityEditor;
using UnityEngine;
using LitEngine;
using LitEngine.ScriptInterface;
using LitEngineEditor;
using System.IO;
public class MenuObject
{
    #region 脚本接口
    static T AddScript<T>(GameObject _object) where T : BehaviourInterfaceBase
    {
        if (_object == null) return null;
        ExportBase.Config.LoadConfig();
        T tscript = _object.AddComponent<T>();

        UnityEngine.SceneManagement.Scene tscene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(tscene);
        return tscript;

    }

    [UnityEditor.MenuItem("GameObject/ScriptInterface/UIInterface", priority = 0)]
    static void AddUIInterface()
    {
        AddScript<UIInterface>(UnityEditor.Selection.activeGameObject);
    }

    [UnityEditor.MenuItem("GameObject/ScriptInterface/BaseInterface", priority = 0)]
    static void AddBaseInterface()
    {
        AddScript<BehaviourInterfaceBase>(UnityEditor.Selection.activeGameObject);
    }

    [UnityEditor.MenuItem("GameObject/ScriptInterface/CET/OnEnableInterface", priority = 0)]
    static void AddOnEnableInterface()
    {
        AddScript<ScriptInterfaceOnEnable>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/CET/OnTriggerInterface", priority = 0)]
    static void AddOnTriggerInterface()
    {
        AddScript<ScriptInterfaceTrigger>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/CET/OnCollisionInterface", priority = 0)]
    static void AddOnCollisionInterface()
    {
        AddScript<ScriptInterfaceCollision>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/CET/CETInterface", priority = 0)]
    static void AddCETInterface()
    {
        AddScript<ScriptInterfaceCET>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/Other/MouseInterface", priority = 0)]
    static void AddMouseInterface()
    {
        AddScript<ScriptInterfaceMouse>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/Other/OnApplicationInterface", priority = 0)]
    static void OnApplicationInterface()
    {
        AddScript<ScriptInterfaceApplication>(UnityEditor.Selection.activeGameObject);
    }
    [UnityEditor.MenuItem("GameObject/ScriptInterface/Other/OnBecameInterface", priority = 0)]
    static void OnBecameInterface()
    {
        AddScript<ScriptInterfaceBecame>(UnityEditor.Selection.activeGameObject);
    }
    #endregion

    [UnityEditor.MenuItem("Export/CreatDirectory For App")]
    static void CreatDirectoryForApp()
    {
        if (!Directory.Exists(ExportBase.Config.sResourcesPath))
            Directory.CreateDirectory(ExportBase.Config.sResourcesPath);
        if (!Directory.Exists(ExportBase.Config.sDefaultFolder))
            Directory.CreateDirectory(ExportBase.Config.sDefaultFolder);
        if (!Directory.Exists(ExportBase.Config.sStreamingBundleFolder))
            Directory.CreateDirectory(ExportBase.Config.sStreamingBundleFolder );
        if (!Directory.Exists(ExportBase.Config.sEditorBundleFolder ))
            Directory.CreateDirectory(ExportBase.Config.sEditorBundleFolder );
    }
}