#region Using directives
using System;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using System.Linq;
using FTOptix.WebUI;
#endregion

public class SearchBrokenDynamicLinks : BaseNetLogic
{
    [ExportMethod]
    public void FindBrokenDynamicLink()
    {
        // Insert code to be executed by the method
        myTask?.Dispose();
        myTask = new LongRunningTask(SearchInProject, LogicObject);
        myTask.Start();
    }

    private void SearchInProject()
    {
        Log.Info("BrokenDynamicLinks", "Searching for broken dynamic links in the project");
        // Get all nodes in the project (variables, UI objects, PLC tags, etc.)
        var projectNodes = Project.Current.FindNodesByType<IUANode>().ToList();
        // Add the root node of the project
        projectNodes.Add(Project.Current);
        // Filter nodes that have a dynamic link
        var projectNodesWithDynamicLinks = projectNodes.Where(node => node.Refs.GetVariable(FTOptix.CoreBase.ReferenceTypes.HasDynamicLink) != null);
        // Counters for broken and uncertain links
        var brokenLinksCount = 0;
        var uncertainLinksCount = 0;

        // Iterate over all nodes with dynamic links and check the link status
        foreach (var nodeWithDynamicLink in projectNodesWithDynamicLinks)
        {
            // Check if the current node has a dynamic link
            var dynamicLinkVariable = nodeWithDynamicLink.Refs.GetVariable(FTOptix.CoreBase.ReferenceTypes.HasDynamicLink);

            // Check if node has a DynamicLink
            if (dynamicLinkVariable == null)
                continue;

            // Retrieve the path of the dynamic link
            var dynamicLinkPath = (string)dynamicLinkVariable.Value;
            Log.Verbose1("BrokenDynamicLinks", $"Checking dynamic link at: \"{Log.Node(nodeWithDynamicLink)}\" with path: \"{dynamicLinkPath}\"");
            // Resolve the dynamic link and get to the target node
            var targetVariable = LogicObject.Context.ResolvePath(nodeWithDynamicLink, dynamicLinkPath);
            if (targetVariable.ResolvedNode == null)
            {
                // Check if the link can be reolved at DesignTime
                var nodePathKind = targetVariable.NodePathKind;

                if (nodePathKind == NodePathKind.Alias || nodePathKind == NodePathKind.Session)
                {
                    Log.Warning("BrokenDynamicLinks", $"Dynamic link at: \"{Log.Node(nodeWithDynamicLink)}\" cannot be resolved at design time ({dynamicLinkPath}) and will be reported as \"uncertain\"");
                    uncertainLinksCount++;
                }
                else
                {
                    if (nodePathKind == NodePathKind.Invalid && dynamicLinkVariable.Children.Any(node => node is FTOptix.CoreBase.Converter))
                    {
                        Log.Warning("BrokenDynamicLinks", $"Node at: \"{Log.Node(nodeWithDynamicLink)}\", has a converter under the dynamic link and cannot be resolved at design time. It will be reported as \"uncertain\"");
                        uncertainLinksCount++;
                    }
                    else
                    {
                        Log.Verbose1("BrokenDynamicLinks", $"Broken dynamic link with NodePathKind: \"{nodePathKind}\"");
                        Log.Error("BrokenDynamicLinks", $"Broken dynamic link at: \"{Log.Node(nodeWithDynamicLink)}\", points to unresolved path: \"{dynamicLinkPath}\"");
                        brokenLinksCount++;
                    }
                }
            }
        }

        Log.Info("BrokenDynamicLinks", $"Found {projectNodesWithDynamicLinks.Count()} dynamic links in the project, {uncertainLinksCount} cannot be resolved at design time, {brokenLinksCount} were broken");
    }

    private LongRunningTask myTask;
}
