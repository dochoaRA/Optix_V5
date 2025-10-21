#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.WebUI;
using FTOptix.System;
using FTOptix.CoreBase;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.OPCUAServer;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.Alarm;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using System.Threading;
#endregion

public class TranslationAxisName : BaseNetLogic
{
    public override void Start()
    {
        var textTag = InformationModel.GetVariable(Owner.GetVariable("AxisNameTextId").Value);
        
        string textid_test = textTag.RemoteRead(); 
        LocalizedText localText = new LocalizedText(textid_test);
        // Get Label text and look up translation
        Owner.GetVariable("AxisName").Value = InformationModel.LookupTranslation(localText);

    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

}
