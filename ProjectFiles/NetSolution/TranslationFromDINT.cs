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
#endregion

public class TranslationFromDINT : BaseNetLogic
{
    public override void Start()
    {
        // Get textID
        string textId = Owner.GetVariable("TextId").Value.Value.ToString();
        // create LocalizedText
        LocalizedText localText = new LocalizedText(textId);
        // Get Label text and look up translation
        Owner.GetVariable("Text").Value = InformationModel.LookupTranslation(localText);
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }
}
