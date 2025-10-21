#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.SQLiteStore;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.DataLogger;
using FTOptix.NativeUI;
using FTOptix.WebUI;
using FTOptix.Store;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.CommunicationDriver;
using FTOptix.NetLogic;
using FTOptix.Core;
using System.Collections.Generic;
#endregion

public class raC_Dvc_IO_1734_Diag_Add_Widgets : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        Add_Widgets(2);
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        DeleteWidgets();
    }


private Dictionary<int, Tuple<string, int,int>> CatNo_1734 = new Dictionary<int, Tuple<string, int,int>>() {
        {42,Tuple.Create("1734-IB8S_SafetyTestOutput",1,12)},
        {43,Tuple.Create("1734-IE4S",1,4)},
        
    };

[ExportMethod]
public void Add_Widgets(int instCount) {
    Container targetContainer = Owner.Get<ColumnLayout>("grp_Home/ChannelSts/ScrollView1/grp_Channels");
    
    IUANode Ref_Tag = null;
    IUANode Ref_Cat = null;
    IUANode Ref_Output = null;
    var dialogBoxAlias = Owner.GetAlias("raSDK1_DialogBox");
    foreach (var aliasChild in dialogBoxAlias.Children) {
        try {
            if (aliasChild.BrowseName == "Ref_Tag" || aliasChild.BrowseName == "Ref_Input") {
                Ref_Tag = InformationModel.Get(((UAVariable)aliasChild).Value);
            } else if (aliasChild.BrowseName == "Ref_Cat") {
                Ref_Cat = InformationModel.Get(((UAVariable)aliasChild).Value);
            } else if (aliasChild.BrowseName == "Ref_Output") {
                Ref_Output = InformationModel.Get(((UAVariable)aliasChild).Value);
            }
        } catch (Exception ex) {
            Log.Error($"Error retrieving node: {ex.Message}");
        }
    }

    var uaVariable1 = Ref_Cat as UAVariable;
    var Cat_Number = uaVariable1?.Value;
    
    var uaVariable = Ref_Tag as UAVariable;
    var variableType = uaVariable?.VariableType;
    var Data_Type = variableType?.BrowseName;
    
    //Banner update
    try
    {
        if (Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0") || Data_Type.StartsWith("AB:1734_IE4S3:I:0")) {
            Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("FaultTag").Value = "ConnectionFaulted";
        } 
    }
    catch (Exception ex)
    {
        Log.Error($"Problem with Banner or FaultTag {ex.Message}");
    }
    

    // Step 1: Find out diCount
    int diCount = 0;
    try {
        //diCount = GetInstCountForVariableType(Cat_Number);
        diCount = (CatNo_1734.TryGetValue(Cat_Number, out var foundTuple)) ? foundTuple.Item2 : 0;
    } catch (Exception ex) {
        Log.Error($"Error getting instance count: {ex.Message}");
        return; // Exit method if error occurs
    }
    int diCount1 = 0; //Added for saftey device banners
    try {
        //diCount1 = GetInstCountForVariableType(Cat_Number);
        diCount1 = (CatNo_1734.TryGetValue(Cat_Number, out var foundTuple)) ? foundTuple.Item3 : 0;
    } catch (Exception ex) {
        Log.Error($"Error getting instance count: {ex.Message}");
        return; // Exit method if error occurs
    }

// Channel fault banner
    for (int j = 0; j < diCount1; j++) {
      try
    {
        if ((j <= 7) && Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0")) {
            Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("Ch_FaultTag"+ j.ToString("D2")).Value = "Pt" + j.ToString("D2") + "Status";//Pt00Status
        }else if (Data_Type.StartsWith("AB:1734_IE4S3:I:0")) {
            Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("Ch_FaultTag"+ j.ToString("D2")).Value = "Ch" + j.ToString() + "InputStatus";//Ch0InputStatus
        }else if ((j == 8) && Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0"))
        {for (int a = 0; a < 4; a++ )
                {
                    Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("Ch_TestOutputFaultTag"+ a.ToString("D2")).Value = "Pt" + a.ToString("D2") + "TestOutputStatus";//Pt00TestOutputStatus 
                }
        } else {
        } 
    }
    catch (Exception ex)
    {
        Log.Error($"Problem with Banner or Ch_FaultTag {ex.Message}");
    }  
    }
        // Step 4: If diCount is less than or equal to 8, add widgets directly to the targetContainer
        for (int j = 0; j < diCount; j++) {
            try {
                 if (Data_Type.StartsWith("AB:1734_IE4S3:I:0") || Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_IE4S3_I_0_Diag>("AOWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        //newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        targetContainer.Add(newWidgetDI);
                    }
                }
            } catch (Exception ex) {
                Log.Error($"Error adding widget directly to target container: {ex.Message}");
            }
        }
    }  
public void DeleteWidgets()
{
    // Get the target container
    var targetContainer = Owner.Get<ColumnLayout>("grp_Home/ChannelSts/ScrollView1/grp_Channels");

    // Check if the container is not null before proceeding
    if (targetContainer != null)
    {
        // Clear the children collection
        targetContainer.Children.Clear();
    }
    else
    {
        Log.Warning("Target container not found.");
    }
}    
}
