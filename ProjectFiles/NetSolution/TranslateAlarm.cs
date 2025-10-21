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
using Microsoft.VisualBasic;
using FTOptix.WebUI;
#endregion

public class TranslateAlarm : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is starte
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void Translate()
    {
        // Insert code to be executed by the method
        var AlarmReason = LogicObject.GetVariable("AlarmReason");
        //AlarmReason.Value = "";
        var AlarmFunction = LogicObject.GetVariable("AlarmFunction");
        //AlarmFunction.Value = "";
        var AlarmMachinePart = LogicObject.GetVariable("AlarmMachinePart");
        //AlarmMachinePart.Value = "";
        var AlarmDeviceTag = LogicObject.GetVariable("AlarmDeviceTag");
        //AlarmDeviceTag.Value = "";
        var AlarmMessageID = LogicObject.GetVariable("AlarmMessageID");
        //AlarmMessageID.Value = "";
        var AlarmMessageNode = LogicObject.GetVariable("AlarmMessage");
        var AlarmMessageBrowsePath = LogicObject.GetVariable("AlarmMessageBrowsePath");
        //string strAlarmMessageBrowsePath= AlarmMessageBrowsePath.Value;
        var AlarmMessageVariable = Project.Current.GetVariable("CommDrivers/RAEtherNet_IPDriver1/CLX/Tags/Program:ProjectSpecific_Panelview/VP_AlarmAdministrator/AlarmForHMI/Message");
        string AlarmMessage = AlarmMessageVariable.RemoteRead();

        var AlarmReasonFunction = LogicObject.GetVariable("AlarmReasonFunction");
        //AlarmReasonFunction.Value = "";

        // Split into max 4 parts
        string[] AlarmMessageParts = AlarmMessage.Split('.');

        try
        {
            var AlarmReasonTextId = AlarmMessageParts[0];
            var AlarmFunctionTextId = AlarmMessageParts[1];
            var AlarmMachinePartTextId = AlarmMessageParts[2];
            // The last part is the message text
            AlarmDeviceTag.Value = AlarmMessageParts[3];

            // Build concatenated string with only non-empty parts
            string messageId = "";

            if (!string.IsNullOrEmpty(AlarmReasonTextId))
                messageId = AlarmReasonTextId;

            if (!string.IsNullOrEmpty(AlarmFunctionTextId))
                messageId = string.IsNullOrEmpty(messageId) ? AlarmFunctionTextId : messageId + "." + AlarmFunctionTextId;

            if (!string.IsNullOrEmpty(AlarmMachinePartTextId))
                messageId = string.IsNullOrEmpty(messageId) ? AlarmMachinePartTextId : messageId + "." + AlarmMachinePartTextId;

            AlarmMessageID.Value = messageId;

            //Necessary, because Lookuptranslation only works with LocalizedTexts and not strings, but .split() only works with strings...

            LocalizedText AlarmReasonLT = new LocalizedText(AlarmReasonTextId);
            AlarmReasonLT.Text = InformationModel.LookupTranslation(AlarmReasonLT).Text;
            LocalizedText AlarmFunctionLT = new LocalizedText(AlarmFunctionTextId);
            AlarmFunctionLT.Text = InformationModel.LookupTranslation(AlarmFunctionLT).Text;
            LocalizedText AlarmMachinePartLT = new LocalizedText(AlarmMachinePartTextId);
            AlarmMachinePartLT.Text = InformationModel.LookupTranslation(AlarmMachinePartLT).Text;

            // Translate each DINT part (Uses ActualID, not Language!)
            AlarmReason.Value = (LocalizedText)AlarmReasonLT; //InformationModel.LookupTranslation(AlarmReasonLT).Text;
            AlarmFunction.Value = (LocalizedText)AlarmFunctionLT;  //InformationModel.LookupTranslation(AlarmFunctionLT).Text;
            AlarmMachinePart.Value = (LocalizedText)AlarmMachinePartLT;  //InformationModel.LookupTranslation(AlarmMachinePartLT).Text;

            /*if (!string.IsNullOrEmpty(AlarmReason.Value) && !string.IsNullOrEmpty(AlarmFunction.Value))
                AlarmReasonFunction.Value = AlarmReason.Value + " / " + AlarmFunction.Value; // Reason / Function
            else if (!string.IsNullOrEmpty(AlarmReason.Value))
                AlarmReasonFunction.Value = AlarmReason.Value; //Reason
            else
                AlarmReasonFunction.Value = AlarmFunction.Value; //Function*/
        }
        catch
        {
            // Alarm message doesnt have the right format
            AlarmMessageID.Value = "";
            AlarmReason.Value = "";
            AlarmFunction.Value = "";
            AlarmMachinePart.Value = "";
            AlarmDeviceTag.Value = "";
        }
        
        

        

    }
}
