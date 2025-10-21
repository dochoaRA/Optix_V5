#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.CoreBase;
using FTOptix.System;
using FTOptix.NetLogic;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.OPCUAServer;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.Alarm;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using FTOptix.WebUI;
#endregion

public class getGroup : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started      
        GetGroup();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    private void GetGroup() 
    {
        //var myUserNodeId = LogicObject.GetAlias("refUser");
        var actualGroupName = LogicObject.GetVariable("actualGroup");
        actualGroupName.Value = "";
        var user = Session.User;
        if (user != null)
        {
            var userGroups = user.Refs.GetObjects(FTOptix.Core.ReferenceTypes.HasGroup, false);
            foreach (var group in userGroups)
            {
                //Log.Info("User is part of group: ", group.BrowseName);
                string strGroup = group.BrowseName.ToString();
                // Expert_temp should appear as Expert. But having a temp version makes removing it a lot easier
                actualGroupName.Value = strGroup.Replace("_temp", "");
            }
        }       
    }    

    [ExportMethod]
    public void Refresh()
    {
        // Insert code to be executed by the method                
        GetGroup();
    }
}
