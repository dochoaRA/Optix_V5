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
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using FTOptix.WebUI;
#endregion


public class TranslateAdjustmentPoints : BaseNetLogic
{

    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        //ImportAdjustmentPointTexts();
        TranslateAdjustmentPointsText();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        //CopyFormatListTextIDsSlowTask = new LongRunningTask(CopyFormatListTextIDs, LogicObject);
        //CopyFormatListTextIDsSlowTask.Start();
        //CopyFormatListTextIDsSlowTask?.Dispose();
        CopyFormatListTextIDs();
    }

   
    [ExportMethod]
    public void TranslateAdjustmentPointsText()
    {
        var TextBoxList = Owner.FindNodesByType<TextBox>();
        foreach (var textBox in TextBoxList)
        {
            if (textBox.BrowseName.Contains("FormatPart"))
            {
                string PartnumberName = ((UAManagedCore.UANode)((UAManagedCore.UANode)textBox.Parent).Parent).BrowseName;
                string ListnumberName = ((UAManagedCore.UANode)textBox.Parent).BrowseName;

                var matchpart = Regex.Match(PartnumberName, @"\d+");
                int FormatListnumber = int.Parse(matchpart.Value);

                var matchList = Regex.Match(ListnumberName, @"\d+");
                int ListnumberValue = int.Parse(matchList.Value);

                // Get the TranslationKey tag
                var TextID_tag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>(
                    $"CommDrivers/RAEtherNet_IPDriver1/RAEtherNet_IPStation1/Tags/Controller Tags/Workfile/FormatList/{FormatListnumber}/Text{ListnumberValue}/TranslationKey");

                var translationKeyObj = TextID_tag.RemoteRead().Value;
                int translationKey = translationKeyObj != null ? Convert.ToInt32(translationKeyObj) : -1;

                if (translationKey != -1 && translationKey != 0)
                {
                    // Lookup translation and set TextBox.LocalizedText
                    // Any other way does not set the TextID 
                    // which results in no further translation (e.g. setting .Text/.TextId directly)
                    textBox.LocalizedText = new LocalizedText(textBox.NodeId.NamespaceIndex, translationKey.ToString());

                }
                else
                {
                    // Get the Value tag and set TextBox.Text to its value
                    var ValueTag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>(
                        $"CommDrivers/RAEtherNet_IPDriver1/RAEtherNet_IPStation1/Tags/Controller Tags/Workfile/FormatList/{FormatListnumber}/Text{ListnumberValue}/Value");
                    var valueObj = ValueTag.RemoteRead().Value;
                    textBox.Text = valueObj != null ? valueObj.ToString() : string.Empty;

                }
            }
        }
    }
    [ExportMethod]
    public void CopyFormatListTextIDs()
    {
        //var stFormatlist = Project.Current.GetVariable("CommDrivers/RAEtherNet_IPDriver1/RAEtherNet_IPStation1/Tags/Controller Tags/Workfile/FormatList");
        // Read the new values
        //var stFormatlist = stWorkfileStructTag.RemoteRead();
        string localizationKey;
        int textID;
        string PartnumberName;
        string ListnumberName;

        var TextBoxList = Owner.FindNodesByType<TextBox>();
        foreach (var textBox in TextBoxList)
        {
            if (textBox.BrowseName.Contains("FormatPart"))
            {

                PartnumberName = ((UAManagedCore.UANode)((UAManagedCore.UANode)textBox.Parent).Parent).BrowseName;
                ListnumberName = ((UAManagedCore.UANode)textBox.Parent).BrowseName;


                // Assuming Partnumber is a string like "MechanicalAdjustmentList_eff1" or similar
                var matchpart = Regex.Match(PartnumberName, @"\d+");
                int FormatListnumber = int.Parse(matchpart.Value);

                // Assuming Listnumber is a string like "DropDownList1" or similar
                var matchList = Regex.Match(ListnumberName, @"\d+");
                int ListnumberValue = int.Parse(matchList.Value);

                //Get Specific Tag from PLC
                var TextID_tag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>($"CommDrivers/RAEtherNet_IPDriver1/RAEtherNet_IPStation1/Tags/Controller Tags/Workfile/FormatList/{FormatListnumber}/Text{ListnumberValue}/TranslationKey");

                if (textBox.LocalizedText.TextId != "" && (ListnumberValue == 1 || ListnumberValue == 2))
                {
                    localizationKey = textBox.LocalizedText.TextId;
                    textID = Convert.ToInt32(localizationKey);

                    TextID_tag.RemoteWrite(textID);
                }
                else
                {
                    TextID_tag.RemoteWrite(-1); // Set to -1 if no translation key is set
                }
                // Set the Value tag to the current TextBox text because it is only written with user selection
                // Which means all values are lost the second time it is opened
                var ValueTag = Project.Current.Get<FTOptix.RAEtherNetIP.Tag>($"CommDrivers/RAEtherNet_IPDriver1/RAEtherNet_IPStation1/Tags/Controller Tags/Workfile/FormatList/{FormatListnumber}/Text{ListnumberValue}/Value");
                string FormatPart = textBox.Text;
                ValueTag.RemoteWrite(FormatPart);
            }

        }

    }

    private LongRunningTask CopyFormatListTextIDsSlowTask;
}
