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

public class raC_Dvc_IO_1734_Home_Add_Widgets : BaseNetLogic
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

private Dictionary<int, Tuple<string, int>> CatNo_1734 = new Dictionary<int, Tuple<string, int>>() {
        {0,Tuple.Create("1734-IA2",2)},
        {1,Tuple.Create("1734-IA4",4)},
        {2,Tuple.Create("1734-IB2",2)},
        {3,Tuple.Create("1734-IB4",4)},
        {4,Tuple.Create("1734-IB8",8)},
        {5,Tuple.Create("1734-IM2",2)},
        {6,Tuple.Create("1734-IM4",4)},
        {7,Tuple.Create("1734-IV2",2)},
        {8,Tuple.Create("1734-IV4",4)},
        {9,Tuple.Create("1734-IV8",8)},
        {10,Tuple.Create("1734-IB4D",4)},
        {11,Tuple.Create("1734-8CFG",8)},
        {12,Tuple.Create("1734-OA2",2)},
        {13,Tuple.Create("1734-OA4",4)},
        {14,Tuple.Create("1734-OB2",2)},
        {15,Tuple.Create("1734-OB2E",2)},
        {16,Tuple.Create("1734-OB2EP",2)},
        {17,Tuple.Create("1734-OB4",4)},
        {18,Tuple.Create("1734-OB4E",4)},
        {19,Tuple.Create("1734-OB8",8)},
        {20,Tuple.Create("1734-OB8E",8)},
        {21,Tuple.Create("1734-OV2E",2)},
        {22,Tuple.Create("1734-OV4E",4)},
        {23,Tuple.Create("1734-OV8E",8)},
        {24,Tuple.Create("1734-OW2",2)},
        {25,Tuple.Create("1734-OW4",4)},
        {26,Tuple.Create("1734-OX2",2)},
        {27,Tuple.Create("1734-IE2C",2)},
        {28,Tuple.Create("1734-IE2V",2)},
        {29,Tuple.Create("1734-IE4C",4)},
        {30,Tuple.Create("1734-IE8C",8)},
        {31,Tuple.Create("1734-IR2",2)},
        {32,Tuple.Create("1734-IR2E",2)},
        {33,Tuple.Create("1734-IT2I",2)},
        {34,Tuple.Create("1734sc-IF4U",4)},
        {35,Tuple.Create("1734sc-IE2CH",2)},
        {36,Tuple.Create("1734sc-IE4CH",4)},
        {37,Tuple.Create("1734-OE2C",2)},
        {38,Tuple.Create("1734-OE2V",2)},
        {39,Tuple.Create("1734-OE4C",4)},
        {40,Tuple.Create("1734sc-OE2CIH",2)},
        {41,Tuple.Create("1734-IB8S",8)},
        {42,Tuple.Create("1734-IB8S_SafetyTestOutput",14)},
        {43,Tuple.Create("1734-IE4S",4)},
        {44,Tuple.Create("1734-OB8S",16)},
        {45,Tuple.Create("1734-OBV2S",4)},
        {60,Tuple.Create("1734-AENT",0)},
        {61,Tuple.Create("1734-AENTR",0)},
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
    if (Data_Type != "RAEtherNetIPTag")
    try
    {
        if (Data_Type.StartsWith("SC:1734sc_IF4U:I:0") || Data_Type.StartsWith("SC:1734sc_IE2CH_HART:I:0") || Data_Type.StartsWith("SC:1734sc_IE4CH_HART:I:0") || 
            Data_Type.StartsWith( "SC:1734sc_OE2CIH_HART:I:0")) 
        {
            Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("FaultTag").Value = "FltStatus";
        }else if (Data_Type.StartsWith("AB:1734_IB8S_Safety2:I:0") || Data_Type.StartsWith("AB:1734_OB8S_Safety2:I:0") || Data_Type.StartsWith("AB:1734_OBV2S_Safety2:I:0")
                || Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0") || Data_Type.StartsWith("AB:1734_IE4S3:I:0"))
        {
            Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("FaultTag").Value = "ConnectionFaulted";
        }else
        {
            Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("FaultTag").Value = "Fault";
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

    // Step 2: Calculate the number of Accordions required
    int accordionCount = 0;
    if (Data_Type.StartsWith("AB:1734_OBV2S_Safety2:I:0")) {
        accordionCount = 2;
    }
    else if (Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0")) {
        accordionCount = 3;
    }else {
        accordionCount = (diCount + 7) / 8; // Calculate the number of Accordions needed
    }

    // Channel fault banner
    for (int j = 0; j < diCount; j++)
    if (Data_Type != "RAEtherNetIPTag") 
    {
      try
    {
        if (Data_Type.StartsWith("AB:1734_IB8S_Safety2:I:0") || (j <= 7) && Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0")) {
            Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("Ch_FaultTag"+ j.ToString("D2")).Value = "Pt" + j.ToString("D2") + "Status";//Pt00Status
        }else if (((j <= 7) && Data_Type.StartsWith("AB:1734_OB8S_Safety2:I:0")) || Data_Type.StartsWith("AB:1734_OBV2S_Safety2:I:0")) {
            Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("Ch_FaultTag"+ j.ToString("D2")).Value = "Pt" + j.ToString("D2") + "OutputStatus";//Pt00OutputStatus
        }else if (Data_Type.StartsWith("AB:1734_IE4S3:I:0")) {
            Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("Ch_FaultTag"+ j.ToString("D2")).Value = "Ch" + j.ToString() + "InputStatus";//Ch0InputStatus
        }else if ((j == 8) && Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0"))
        {for (int a = 0; a < 4; a++ )
            {
            Owner.Get<raC_5_05_raC_Dvc_1734_XXXX_Banner>("grp_Home/Banner").GetVariable("Ch_TestOutputFaultTag"+ a.ToString("D2")).Value = "Pt" + a.ToString("D2") + "TestOutputStatus";//Pt00TestOutputStatus 
            }
        }else{ 
        }  
    }
    catch (Exception ex)
    {
        Log.Error($"Problem with Banner or Ch_FaultTag {ex.Message}");
    }  
    }
    // Step 3: If diCount is greater than 8, create Accordions
    if (accordionCount > 1) {
        for (int i = 0; i < accordionCount; i++) {
            try {
                // Create a new accordion panel
                Container accordionPanel = InformationModel.Make<Accordion>("AccordionPanel" + i.ToString());
                accordionPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                accordionPanel.VerticalAlignment = VerticalAlignment.Top;
                accordionPanel.BottomMargin = 8;
                targetContainer.Add(accordionPanel);
                
                // Add header label
                Label Header_Label = InformationModel.Make<Label>("AccordionPanel_HL" + i.ToString());
                var Header_Height = InformationModel.Make<Label>("AccordionPanel_HL" + i.ToString());
                accordionPanel.Get<AccordionHeader>("Header")?.Add(Header_Label);
                accordionPanel.Get<AccordionHeader>("Header")?.Add(Header_Height);
                Header_Height.Height = 30;
                Header_Label.TopMargin = 4;
                
                // Add content column layout
                ColumnLayout Content_CL = InformationModel.Make<ColumnLayout>("AccordionPanel_CL" + i.ToString());
                Content_CL.HorizontalAlignment = HorizontalAlignment.Stretch;
                Content_CL.VerticalAlignment = VerticalAlignment.Top;
                Content_CL.VerticalGap = 0; Content_CL.LeftMargin = 8; Content_CL.TopMargin = 8; Content_CL.RightMargin = 8; Content_CL.BottomMargin = 8;
                accordionPanel.Get<AccordionContent>("Content")?.Add(Content_CL);
                
                // Set the title text
                var HL = accordionPanel.Get<Label>("Header/AccordionPanel_HL" + i.ToString());
                string title = "";
                int startIndex = 0; int endIndex = 0;
                if ((i == 1) & (Data_Type.StartsWith("AB:1734_OB8S_Safety2:I:0") || Data_Type.StartsWith("AB:1734_OBV2S_Safety2:I:0") ))
                {
                   startIndex = 0;
                   endIndex = Math.Min(startIndex + 8, diCount);
                }
                else if ((i == 1) & (Data_Type.StartsWith("AB:1734_OBV2S_Safety2:I:0")))
                {
                   startIndex = 0;
                   endIndex = 2;
                }else if ((i == 0) & (Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0")))
                {
                    startIndex = 0;
                    endIndex = Math.Min(startIndex + 8, diCount);
                }else if ((i == 1) & (Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0")))
                {
                    startIndex = 0;
                    endIndex = 4;
                }else if ((i == 2) & (Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0")))
                {
                    startIndex = 0;
                    endIndex = 2;
                }else
                {
                   startIndex = 0;
                   endIndex = Math.Min(startIndex + 8, diCount);
                }

                if ((i == 1) & (Data_Type.StartsWith("AB:1734_OB8S_Safety2:I:0") || Data_Type.StartsWith("AB:1734_OBV2S_Safety2:I:0")))
                {
                    title = " Readback - Ch" + startIndex.ToString() + "..." +  (endIndex -1).ToString();
                }else if ((i == 0) & (Data_Type.StartsWith("AB:1734_OB8S_Safety2:I:0") || Data_Type.StartsWith("AB:1734_OBV2S_Safety2:I:0")) || ((i == 1) & (Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0" )))) {
                    title = " Output - Ch" + startIndex.ToString() + "..." +  (endIndex - 1).ToString();
                }else if ((i == 0) & (Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0"))) {
                    title = " Input - Ch" + startIndex.ToString() + "..." +  (endIndex - 1).ToString();
                }else if ((i == 2) & (Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0"))) {
                    title = " Muting Lamp - Ch" + (startIndex + 1).ToString() + "..." +  (endIndex + 1).ToString();
                }
                HL.Text = title;
                HL.Style = "Heading";
                
                // Add widgets to the accordion container
                for (int j = startIndex; j < endIndex; j++) {
                    if (Data_Type.StartsWith("AB:1734_OB8S_Safety2:I:0") || Data_Type.StartsWith("AB:1734_OBV2S_Safety2:I:0")) {
                        var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_OB8S_Safety2_I_0>("DIWidget" + j.ToString());
                        if (newWidgetDI != null) {
                            newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                            newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                            newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        if (i==1)
                        {          
                            newWidgetDI.GetVariable("Channel_DataTag_Member").Value = "Ref_Tag/" + "Pt" + j.ToString("D2")+"Readback"; //Pt00Readback 
                            newWidgetDI.GetVariable("Cfg_Fault").Value = false;                             
                        }
                        else
                        {
                            newWidgetDI.GetVariable("Channel_DataTag_Member").Value = "Ref_Output/" + "Pt" + j.ToString("D2")+"Data"; //Pt00Data
                            newWidgetDI.GetVariable("Channel_FaultTag_Member").Value = "Ref_Tag/" + "Pt" + j.ToString("D2") + "OutputStatus"; //Pt00OutputStatus
                            newWidgetDI.GetVariable("Cfg_Fault").Value = true;
                        }
                            Content_CL?.Add(newWidgetDI);
                        }
                    }else if (Data_Type.StartsWith("AB:1734_IB8S_Safety5:I:0")) {
                        if (i == 0) {
                            var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_IB8S_Safety5_I_0_Inputs>("AIWidget" + j.ToString());
                            if (newWidgetDI != null) {
                            newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                            newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                            newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString("D2");
                            newWidgetDI.GetVariable("Cfg_Label").Value = "Inputs";
                            newWidgetDI.GetVariable("Channel_DataTag_Member").Value = "Ref_Tag/" + "Pt" + j.ToString("D2") + "Data";//Pt00Data
                            newWidgetDI.GetVariable("Channel_FaultTag_Member").Value = "Ref_Tag/" + "Pt" + j.ToString("D2") + "Status";//Pt00Status
                            Content_CL?.Add(newWidgetDI);
                            }
                        }else if (i == 1) {
                            var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_IB8S_Safety5_I_0_Inputs>("AIWidget" + j.ToString());
                            if (newWidgetDI != null) {
                            newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                            newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                            newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString("D2");
                            newWidgetDI.GetVariable("Cfg_Label").Value = "Test Outputs";
                            newWidgetDI.GetVariable("Channel_DataTag_Member").Value = "Ref_Output/" + "Test" + j.ToString("D2")+ "Data";//Test00Data
                            newWidgetDI.GetVariable("Channel_FaultTag_Member").Value = "Ref_Tag/" + "Pt" + j.ToString("D2") + "TestOutputStatus";//Pt00TestOutputStatus
                            Content_CL?.Add(newWidgetDI);
                            }
                        }else if (i == 2) {
                            int[] channelNumbers = { 1, 3 };
                            var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_IB8S_Safety5_I_0_Inputs>("AIWidget" + j.ToString());
                            if (newWidgetDI != null) {
                            newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                            newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                            newWidgetDI.GetVariable("Cfg_ChannelNo").Value = channelNumbers[j].ToString();
                            newWidgetDI.GetVariable("Cfg_Label").Value = "Muting Lamp Outputs";
                            newWidgetDI.GetVariable("Channel_DataTag_Member").Value = "Ref_Tag/" + "Muting" +channelNumbers[j].ToString("D2") +"Status";//Muting01Status
                            newWidgetDI.GetVariable("Channel_FaultTag_Member").Value = "";
                            Content_CL?.Add(newWidgetDI);
                            }
                        }   
                    }else if (Data_Type.StartsWith("RAEtherNetIPTag")) {
                            var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_RackOptimization>("DIWidget" + j.ToString());
                            if (newWidgetDI != null) {
                            newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                            newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                            newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                            Content_CL?.Add(newWidgetDI);
                            }
                    }
                }
            } catch (Exception ex) {
                Log.Error($"Error creating accordion panel: {ex.Message}");
            }
        }
    } else {
        // Step 4: If diCount is less than or equal to 8, add widgets directly to the targetContainer
        for (int j = 0; j < diCount; j++) {
            try {
                if(Data_Type.StartsWith("AB:1734_DI8:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_DI8_I_0>("DIWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        targetContainer.Add(newWidgetDI);
                    }
                }else if (Data_Type.StartsWith("AB:1734_DI4Diags:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_DI4Diags_I_0>("DIWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        targetContainer.Add(newWidgetDI);
                    }
                }else if (Data_Type.StartsWith("AB:1734_DO2:I:0") || Data_Type.StartsWith("AB:1734_DOB8:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_DOB8_I_0>("DOWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        targetContainer.Add(newWidgetDI);
                    }
                }else if (Data_Type.StartsWith("AB:1734_IE2:I:0") || Data_Type.StartsWith("AB:1734_IE4:I:0") || Data_Type.StartsWith("AB:1734_IE8:I:0") || Data_Type.StartsWith("AB:1734_IR2:I:0")
                        || Data_Type.StartsWith("AB:1734_IT2I:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_IE8_I_0>("AIWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        newWidgetDI.GetVariable("Channel_DataTag_Member").Value ="Ch" + j.ToString()+"Data";
                        targetContainer.Add(newWidgetDI);
                    }
                }else if (Data_Type.StartsWith("SC:1734sc_IF4U:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_SC_1734sc_IF4U_I_0>("AIWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        newWidgetDI.GetVariable("Channel_DataTag_Member").Value ="Ch" + j.ToString()+"Data";
                        targetContainer.Add(newWidgetDI);
                    }
                }else if (Data_Type.StartsWith("AB:1734_OE2:I:0") || Data_Type.StartsWith("AB:1734_OE4:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_OE4_I_0>("AOWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
						newWidgetDI.GetVariable("Channel_DataTag_Member").Value ="Ch" + j.ToString()+"Data";
                        targetContainer.Add(newWidgetDI);
                    }    
                }else if (Data_Type.StartsWith("SC:1734sc_IF4U:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_SC_1734sc_IF4U_I_0>("AIWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        targetContainer.Add(newWidgetDI);
                    }
                }else if (Data_Type.StartsWith("SC:1734sc_IE2CH_HART:I:0") || Data_Type.StartsWith("SC:1734sc_IE4CH_HART:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_SC_1734sc_IE4CH_HART_I_0>("AIWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        newWidgetDI.GetVariable("Channel_DataTag_Member").Value ="Ch" + j.ToString()+"Data";
                        targetContainer.Add(newWidgetDI);
                    }
                }else if (Data_Type.StartsWith("SC:1734sc_OE2CIH_HART:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_SC_1734sc_OE2CIH_HART_I_0>("AIWidget" + j.ToString());
                   if  (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
						newWidgetDI.GetVariable("Channel_DataTag_Member").Value ="Ch" + j.ToString()+"_Output";
                        targetContainer.Add(newWidgetDI);
                    }
                }else if (Data_Type.StartsWith("AB:1734_IB8S_Safety2:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_IB8S_Safety2_I_0>("DIWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString("D2");
                        newWidgetDI.GetVariable("Channel_DataTag_Member").Value ="Pt" + j.ToString("D2")+"Data";
                        newWidgetDI.GetVariable("Channel_FaultTag_Member").Value = "Pt" + j.ToString("D2")+"Status";
                        targetContainer.Add(newWidgetDI);
                    }
                }else if (Data_Type.StartsWith("AB:1734_IE4S3:I:0")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_IE4S3_I_0_Home>("DIWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        newWidgetDI.GetVariable("Channel_DataTag_Member").Value ="Ch" + j.ToString()+"Data";
                        newWidgetDI.GetVariable("Channel_FaultTag_Member").Value = "Ch" + j.ToString()+"InputStatus";
                        targetContainer.Add(newWidgetDI);
                    }
                }else if (Data_Type.StartsWith("RAEtherNetIPTag")) {
                    var newWidgetDI = InformationModel.Make<raC_Dvc_AB_1734_RackOptimization>("DIWidget" + j.ToString());
                    if (newWidgetDI != null) {
                        newWidgetDI.VerticalAlignment = VerticalAlignment.Top;
                        newWidgetDI.HorizontalAlignment = HorizontalAlignment.Stretch;
                        newWidgetDI.GetVariable("Cfg_ChannelNo").Value = j.ToString();
                        targetContainer.Add(newWidgetDI);
                    }
                }
            } catch (Exception ex) {
                Log.Error($"Error adding widget directly to target container: {ex.Message}");
            }
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
