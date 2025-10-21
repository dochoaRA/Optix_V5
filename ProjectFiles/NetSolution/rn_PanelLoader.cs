#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using System.Collections.Generic;
using System.Security.Cryptography;
using FTOptix.WebUI;
#endregion


public class rn_PanelLoader : BaseNetLogic
{
    private UAManagedCore.NodeId current_screen;
    private Stack<NodeId> backwardsPanelStack; // storing historical panel NodeId for Backward Button
    private Stack<NodeId> forwardsPanelStack; // storing historical panel NodeId for Forward Button

    /*  The "panelList" is defined and declared as a public static because we want it to be accessed
     *  by other classes for using a popup (e.g., listbox) to display the historical panels. 
     */
    public static List<NodeId> panelList = new List<NodeId>();
    private int initial;

    public override void Start()
    {
        initial = 0;
        backwardsPanelStack = new Stack<NodeId>(); 
        forwardsPanelStack = new Stack<NodeId>(); 
        var panelLoader = Owner as PanelLoader;
        if (panelLoader == null)
        {
            Log.Error("BackProvider", "Panel loader not found");
            return;
        }
        panelLoader.PanelVariable.VariableChange += PanelVariable_VariableChange;
    }
   
    public override void Stop()
    {
        var panelLoader = Owner as PanelLoader;
        if (panelLoader == null)
        {
            Log.Error("BackProvider", "Panel loader not found");
            return;
        }

        panelLoader.PanelVariable.VariableChange -= PanelVariable_VariableChange;
    }

    private void PanelVariable_VariableChange(object sender, VariableChangeEventArgs e) // Panel Change
    {
        var oldPanel = InformationModel.Get(e.OldValue); // Retreiving the old panel's node Id
        NodeId oldPanelNodeId = e.OldValue;

        if (initial == 1)  
        {
            backwardsPanelStack.Push(oldPanelNodeId); // Add to Backward stack after first panel is changed
        }
        else
        {
            initial = 1;
        }
        current_screen = e.NewValue; // Current screen node Id
        AddNewPanelToList(e.NewValue); // Add to List of Panels for Panel History

        var Back_enabled = LogicObject.GetVariable("Back_enabled"); //If true, different icon is displayed
        Back_enabled.Value = true; //enabled as a screen has just been added to the stack
    }

    [ExportMethod]
    public void BackwardNav() // Backward Button Press
    {
        if (backwardsPanelStack.Count != 0) // If nothing is in Backward stack, then return
        {
            var panelLoader = Owner as PanelLoader;
            if (panelLoader == null)
            {
                Log.Error("BackProvider", "Panel loader not found");
                return;
            }

            var Forward_enabled = LogicObject.GetVariable("Forward_enabled"); //If true, different icon is displayed
            var Back_enabled = LogicObject.GetVariable("Back_enabled"); //If true, different icon is displayed

            forwardsPanelStack.Push(current_screen); // Add current screen to Forward stack after the Backwards Button is pressed
            Forward_enabled.Value = true; //always enabled as a screen has just been added to the stack
            current_screen = backwardsPanelStack.Peek(); // Update current panel to the top of Backward stack
            var panelNodeId = backwardsPanelStack.Pop(); // The current panel node Id is stored into a variable while removing the top of the Backward Stack
            panelLoader.PanelVariable.VariableChange -= PanelVariable_VariableChange;
            panelLoader.ChangePanel(panelNodeId, NodeId.Empty); // Change panel to previous panel
            panelLoader.PanelVariable.VariableChange += PanelVariable_VariableChange;
            AddNewPanelToList(panelNodeId); // Add to List of Panels for Panel History
            
            if (backwardsPanelStack.Count == 0) 
            {
                Back_enabled.Value = false; // If the Backward stack is empty, disable the Backward button
            }
            else
            {
                Back_enabled.Value = true; // If the Backward stack is not empty, enable the Backward button
            }
        }
    }

    [ExportMethod]
    public void ForwardNav() // Forward Button Press
    {
        if (forwardsPanelStack.Count != 0) // If nothing is in Forward stack, then return
        {
            var panelLoader = Owner as PanelLoader;
            if (panelLoader == null)
            {
                Log.Error("ForwardProvider", "Panel loader not found");
                return;
            }

            var Forward_enabled = LogicObject.GetVariable("Forward_enabled"); //If true, different icon is displayed
            var Back_enabled = LogicObject.GetVariable("Back_enabled"); //If true, different icon is displayed

            backwardsPanelStack.Push(current_screen); // Add current screen to Backward stack after the Forwards Button is pressed
            Back_enabled.Value = true; //always enabled as a screen has just been added to the stack
            current_screen = forwardsPanelStack.Peek(); // Update current panel to the top of Forward stack
            var panelNodeId = forwardsPanelStack.Pop(); // The current panel node Id is stored into a variable while removing the top of the Forward Stack
            panelLoader.PanelVariable.VariableChange -= PanelVariable_VariableChange;
            panelLoader.ChangePanel(panelNodeId, NodeId.Empty); // Change panel to previous panel
            panelLoader.PanelVariable.VariableChange += PanelVariable_VariableChange;
            AddNewPanelToList(panelNodeId); // Add to List of Panels for Panel History

            if (forwardsPanelStack.Count == 0) 
            {
                Forward_enabled.Value = false; // If the Backward stack is empty, disable the Backward button
            }
            else
            {
                Forward_enabled.Value = true; // If the Backward stack is not empty, enable the Backward button
            }
        }
    }

    private void AddNewPanelToList(NodeId id) // Add panel to List
    {
        if (panelList.Count == 0)
        { 
            panelList.Add(id); // Add current panel to List and exit
            return;
        }

        //here we hard coded 15 as the maximum number of panels in the popup list
        if (panelList.Count <= 15) // If the amount of panels in the List is between 1 and 15
        {
            var newPanel = InformationModel.Get(id); // Retreiving the panel's node Id
            int foundIndex = -1;

            for (int i = 0; i < panelList.Count; i++) // Going through every panel in panelList
            {
                var oldPanel = InformationModel.Get(panelList[i]);
                if (oldPanel != null) 
                {
                    // we found a duplicate and store its index that is used to remove the old panel node id later
                    if (string.Equals(newPanel.BrowseName, oldPanel.BrowseName, StringComparison.OrdinalIgnoreCase)) 
                    {
                        foundIndex = i; // Duplicates are stored at index i
                        break;
                    }                    
                }
            }
            if (foundIndex == -1) // If a duplicate is not found
            {
                panelList.Add(id); // Add panel to List
            }
            else // A duplicate is found
            {
                panelList.RemoveAt(foundIndex); // Remove duplicate
                panelList.Add(id); // Add panel to list
            }
        }
        else // Amount of panels in List > 15
        {
            panelList.RemoveAt(0); // Remove panel at beginning of List
        }
    }

    [ExportMethod]
    public void LoadHistoricalPanel(string panelName) // Load Panel from history
    {
        foreach (var id in panelList) // For each id in the List
        {
            var panel = InformationModel.Get(id);  // Retreiving the panel's node Id
            if (panel != null) // Make sure a panel exists for the given node id
            {
                // The selected panel does exist in the historical queue.
                if (string.Equals(panel.BrowseName, panelName,StringComparison.OrdinalIgnoreCase)) 
                {
                    var panelLoader = Owner as PanelLoader;
                    panelLoader.ChangePanel(id, NodeId.Empty); // Change panel to the selected one 
                    break;
                }
            }
        }
    }
}
