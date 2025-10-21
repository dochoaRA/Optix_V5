#region Using directives
using System;
using UAManagedCore;
using FTOptix.NetLogic;
using FTOptix.WebUI;
using Microsoft.VisualBasic;
using FTOptix.HMIProject;
using FTOptix.Core;
using FTOptix.UI;


#endregion

public class SaveLastNodeId : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started

        //At startup, check whether there is an alias node in the Screen AlarmDetail
        var AliasNode = Owner.GetVariable("SelectedAlarm");

        //if yes, write it to LastselectedNodeId
        var lastSelectedNodeId = Session.Children.GetVariable("LastSelectedAlarmNodeId");
        var ChangeToAlarmList = LogicObject.GetVariable("ChangeToAlarmList");

        if (AliasNode.Value.Value != null) //Aliasnode.Value is a container containing the NodeId (.Value)
        {
            lastSelectedNodeId.Value = AliasNode.Value;
            ChangeToAlarmList.Value = false;
        }
        else
        {
            //if not, reload the last one
            //AliasNode.Value = lastSelectedNodeId.Value;
            //Doesnt work currently - reload alarm list

            ChangeToAlarmList.Value = true;
        }

    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }
}
