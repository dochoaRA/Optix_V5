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

public class NumpadLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void CheckInput()
    {
        // Insert code to be executed by the method

        var InputBox = (FTOptix.UI.TextBox)Owner.Children.Get("PreEditSpinner").Children.Get("Input_Box");
        var bufferValue = InputBox.Text;
        bool numeric = isNumeric(bufferValue);
        bool evaluable = !numeric && canEvaluate(bufferValue);

        Owner.Children.GetVariable("bNumeric").Value = numeric;
        Owner.Children.GetVariable("bCanEvaluate").Value = evaluable;

    }

    private bool isNumeric(object value)
    {
        if (value == null)
            return false;

        // If value is a number type
        if (value is double || value is float || value is int || value is long || value is decimal)
            return !double.IsNaN(Convert.ToDouble(value)) && !double.IsInfinity(Convert.ToDouble(value));

        // If value is a string
        string str = value.ToString().Replace(",", ".");
        if (!System.Text.RegularExpressions.Regex.IsMatch(str, @"^-?[0-9.]*$"))
            return false;

        double num;
        return str.Trim().Length != 0 && double.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out num);
    }

    private bool canEvaluate(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        try
        {
            string expr = expression.Replace(",", ".").Replace("÷", "/");
            var result = new System.Data.DataTable().Compute(expr, null);
            return isNumeric(result);
        }
        catch
        {
            return false;
        }
    }

    [ExportMethod]
    public void Calculate()
    {
        // Insert code to be executed by the method
        var InputBox = (FTOptix.UI.TextBox)Owner.Children.Get("PreEditSpinner").Children.Get("Input_Box");
        var sBufferExpression = InputBox.Text;


        try
        {
            // Use DataTable for basic evaluation
            var dt = new System.Data.DataTable();
            var result = dt.Compute(sBufferExpression, "");

            // There should only be one, but it is the only way I found to address the element
            // Textbox as an element packs all its variables together and they can only be addressed 
            // as a subElement of the Textbox object or one string with all together

            // Beides geht und ändert test1
            var resultVar = Owner.Children.Get("NumpadLogic").GetVariable("InputText");
            resultVar.Value = result.ToString();
    
            /*var TextBoxPreEdit = Owner.Children.Get("PreEditSpinner").FindNodesByType<TextBox>();

            foreach (var textBox in TextBoxPreEdit)
            {
                textBox.Text = result.ToString();
            }*/

            var TextBoxPreEdit2 = Owner.Children.Get("PreEditSpinner").FindNodesByType<TextBox>();
            foreach (var textBox in TextBoxPreEdit2)
            {
                var test1 = textBox.Text;
            }

        }
        catch
        {
            Log.Warning("Keypad input", "invalid expression encountered");
        }
    }

    [ExportMethod]
    public void SetUpNumped()
    {
        try
        {
            // Insert code to be executed by the method
            // Get the variable 'ComponentNodeId' from the Owner's children
            var componentNodeIdVar = Owner.Children.GetVariable("ComponentNodeId");

            // Extract the NodeId string from the variable's value
            var nodeIdString = (UAManagedCore.NodeId)componentNodeIdVar.Value;

            // Get the node from the information model using the NodeId string
            var AliasNode = InformationModel.Get(nodeIdString);

            // Set Title
            var Label = (LocalizedText)AliasNode.Owner.Owner.Owner.Children.GetVariable("Label").Value;
            Owner.Children.GetVariable("Label").Value = InformationModel.LookupTranslation(Label).Text;

            // Set Unit if available
            var Unit = (LocalizedText)AliasNode.Owner.Owner.Owner.Children.GetVariable("Unit").Value;
            if (!string.IsNullOrEmpty(Unit.Text))
            {
                Owner.Children.GetVariable("Unit").Value = InformationModel.LookupTranslation(Unit).Text;
            }

            // Set Min and Max if available

            //try as not all have minMax
            try
            {
                var Min = (Int32)AliasNode.Owner.Owner.Owner.Children.GetVariable("Min").Value;
                Owner.Children.GetVariable("Min").Value = Min;

                var Max = (Int32)AliasNode.Owner.Owner.Owner.Children.GetVariable("Max").Value;
                Owner.Children.GetVariable("Max").Value = Max;
            }
            catch
            {
                //Component without MinMax
            }

        }
        catch
        {
            //For whatever reason this is called at times where no numpad should be called and then it fails...
        }
        

        
        

        // When openeing the same thing again, do the titles stay or do I have to reset the control variabel?
    }
}
