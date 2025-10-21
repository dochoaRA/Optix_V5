#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.System;
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

public class TranslateFormatList : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        /*var EditLabelList = Owner.FindNodesByType<EditableLabel>();
        foreach (var label in EditLabelList)
        {
            // Get the LocalizedText variable from the Label
            string Key = label.LocalizedText.TextId;
            LocalizedText AdjustmentPoint = new LocalizedText(Key);
            label.Text = InformationModel.LookupTranslation(AdjustmentPoint).Text;
        }*/

    }
    
    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }
}
