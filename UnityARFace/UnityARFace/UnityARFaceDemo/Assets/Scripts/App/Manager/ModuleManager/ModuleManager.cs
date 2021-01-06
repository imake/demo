using System.Collections.Generic;
using UnityEngine;

public class ModuleManager : Singleton<ModuleManager>
{
    private Dictionary<string, BaseModel> modelDict = new Dictionary<string, BaseModel>();
    private Dictionary<string, BaseCtrl> ctrlDict = new Dictionary<string, BaseCtrl>();
    private Dictionary<string, BaseUICtrl> uiCtrlDict = new Dictionary<string, BaseUICtrl>();

    public BaseCtrl GetCtrl(string ctrlName)
    {
        BaseCtrl ctrl = null;
        if (!ctrlDict.TryGetValue(ctrlName, out ctrl))
        {
            Debug.LogError("[ModuleManager]No Have This Ctrl " + ctrlName);
        }
        return ctrl;
    }

    public BaseModel GetModel(string modelName)
    {
        BaseModel model = null;
        if (!modelDict.TryGetValue(modelName, out model))
        {
            Debug.LogError("[ModuleManager]No Have This Model " + modelName);
        }
        return model;
    }

    public void ResetModel(string modelName)
    {
        BaseModel model = GetModel(modelName);
        model.Reset();
    }

    public void AddModel(string modelName, BaseModel model)
    {
        model.modelName = modelName;
        modelDict[modelName] = model;
    }

    public void AddCtrl(string ctrlName, BaseCtrl ctrl)
    {
        ctrl.ctrlName = ctrlName;
        ctrlDict[ctrlName] = ctrl;
    }

    public void AddUICtrl(string ctrlName, BaseUICtrl uiCtrl)
    {
        uiCtrl.ctrlName = ctrlName;
        uiCtrlDict[ctrlName] = uiCtrl;
    }

    public void ResetAllModel()
    {
        foreach (BaseModel model in modelDict.Values)
        {
            model.Reset();
        }
    }

    public void ModuleReadData()
    {
        foreach (BaseModel model in modelDict.Values)
        {
            model.ReadData();
        }
    }

    public void DisposeAllModel()
    {
        foreach (BaseModel model in modelDict.Values)
        {
            model.Dispose();
        }
        modelDict.Clear();
    }

    public void DisposeAllCtrl()
    {
        foreach (BaseCtrl ctrl in ctrlDict.Values)
        {
            ctrl.Dispose();
        }
        ctrlDict.Clear();
        foreach (BaseUICtrl uiCtrl in uiCtrlDict.Values)
        {
            uiCtrl.Dispose();
        }
        uiCtrlDict.Clear();
    }

    public void DisposeAllModule()
    {
        DisposeAllModel();
        DisposeAllCtrl();
    }

    public override void Dispose()
    {
        base.Dispose();

        modelDict = null;
        ctrlDict = null;
        uiCtrlDict = null;
    }
}
