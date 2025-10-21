#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.WebUI;
#endregion

public class NodesCounterDialogLogic : BaseNetLogic
{
    public override void Start()
    {
        // Using an Alias we fetch the element where we need to count the nodes
        var rootElementAlias = Owner.GetVariable("RootElement") ?? throw new CoreConfigurationException("Alias 'RootElement' not found in the DialogBox");
        // The target of the Alias is the value of the Alias variable (which is a NodeId)
        var rootElement = rootElementAlias.Value ?? throw new CoreConfigurationException("'RootElement' alias was empty, cannot calculate nodes");
        // Get the target element from the Information Model (using the NodeId from the Alias)
        var targetElement = InformationModel.Get(rootElement) ?? throw new CoreConfigurationException("Cannot find the target element in the Information Model");

        // If the target element is a PanelLoader or NavigationPanel, we need to get the current panel so we can listen to changes
        if (targetElement is PanelLoader || targetElement is NavigationPanel)
            currentPage = targetElement.GetVariable("CurrentPanel");
        else
            currentPage = rootElementAlias;

        // Listen to changes in the current page variable
        // When the source node is a PanelLoader or NavigationPanel,
        // the CurrentPanel variable is changed when the user navigates to a different page
        currentPage.VariableChange += CurrentPage_VariableChange;
        CountNodesInPage();
    }

    private void CurrentPage_VariableChange(object sender, VariableChangeEventArgs e)
    {
        // When the current page changes, count the nodes in the new page
        CountNodesInPage();
    }

    public override void Stop()
    {
        // Stop listening to changes in the current page variable
        currentPage.VariableChange -= CurrentPage_VariableChange;
    }

    [ExportMethod]
    public void CountNodesInPage()
    {
        // Enable loading animation
        var loadingAnimation = Owner.Get<Rectangle>("Loading");
        loadingAnimation.Visible = true;

        // Get the root element from the Information Model of the project
        var rootElement = InformationModel.Get(currentPage.Value) ?? throw new CoreConfigurationException("Cannot find the target element in the Information Model");

        // Reset counters
        nodesCount = 0;
        uiObjectsCount = 0;
        variablesCount = 0;
        dynamicLinksCount = 0;
        convertersCount = 0;

        // Recursively count child nodes of the root element
        foreach (var child in rootElement.Children)
        {
            RecursivelyCountNodes(child);
        }

        // Set results to UI
        Owner.Get<Label>("NodesCountValue").Text = nodesCount.ToString();
        Owner.Get<Label>("ObjectsCountValue").Text = uiObjectsCount.ToString();
        Owner.Get<Label>("VariablesCountValue").Text = variablesCount.ToString();
        Owner.Get<Label>("DynamicLinksCountValue").Text = dynamicLinksCount.ToString();
        Owner.Get<Label>("ConvertersCountValue").Text = convertersCount.ToString();

        // Disable loading animation
        loadingAnimation.Visible = false;
    }

    private void RecursivelyCountNodes(IUANode element)
    {
        // The head node is also counted
        ++nodesCount;

        if (element is BaseUIObject)
        {
            // Count UI objects
            ++uiObjectsCount;
        }
        
        if (element.NodeClass == NodeClass.Variable)
        {
            // Count variables
            ++variablesCount;
        }

        // Count dynamic links
        if (element.Refs.GetNode(FTOptix.CoreBase.ReferenceTypes.HasDynamicLink) != null)
        {
            ++dynamicLinksCount;
            ++nodesCount;
        }

        // Count converters
        if (element.Refs.GetNode(FTOptix.CoreBase.ReferenceTypes.HasConverter) != null)
        {
            ++convertersCount;
            ++nodesCount;
        }

        if (element.Children.Count > 0)
        {
            // Go down recursively
            foreach (var child in element.Children)
                RecursivelyCountNodes(child);
        }
    }

    private IUAVariable currentPage;
    private int nodesCount;
    private int uiObjectsCount;
    private int variablesCount;
    private int dynamicLinksCount;
    private int convertersCount;
}
