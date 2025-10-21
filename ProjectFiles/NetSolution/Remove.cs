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
using System.Collections.Generic;
using System.Linq;
#endregion

public class Remove : BaseNetLogic
{
    [ExportMethod]
    public void Method1()
    {
        // Insert code to be executed by the method
        StuffRemoveDictionaryTranslations(
            Owner.GetVariable("Rovema_Dictionary(DO_NOT_OPEN)"),
            new List<string> { "0" }
        );
    }



    private void StuffRemoveDictionaryTranslations(IUAVariable dictionary, List<string> keysToRemove)
        {
            // A big dictionary may take a long processing time, usage of a LongRunningTask is recommended
            if (dictionary == null || dictionary.Value.Value == null || keysToRemove == null || keysToRemove.Count <= 0)
                return;
            string[,] actualDictionaryValues;
            try
            {
                actualDictionaryValues = dictionary.Value.Value as string[,];
            }
            catch
            {
                //Maybe is not a dictionary, so is better to stop execution here
                return;
            }
            List<int> rowsToRemove = new List<int>();
            const int defaultValue = 0;
            foreach(string keyToRemove in keysToRemove) 
            {
                if (string.IsNullOrEmpty(keyToRemove))
                {
                    Log.Warning(LogicObject.BrowseName, "Missing key, skipped");
                    continue;
                }
                int rowToDelete = Enumerable
            .Range(0, actualDictionaryValues.GetLength(0))
            .Where(x => actualDictionaryValues[x, 0] == keyToRemove)
            .FirstOrDefault(defaultValue);
                if (rowToDelete == defaultValue)
                {
                    Log.Warning(LogicObject.BrowseName, $"Key {keyToRemove} not exist, cannot edit the dictionary, use Add method");
                    continue;
                }
                rowsToRemove.Add(rowToDelete);
            }
            if (rowsToRemove.Count >= actualDictionaryValues.GetLength(0))
            {
                Log.Error(LogicObject.BrowseName, "The rows to be removed are equal to or more than the dictionary entries with header, impossible to continue");
                return;
            }
            string[,] newDictionaryValues = new string[actualDictionaryValues.GetLength(0) - rowsToRemove.Count, actualDictionaryValues.GetLength(1)];
            int rowToWrite = 0;
            for (int i = 0; i < actualDictionaryValues.GetLength(0); i++) 
            {
                if (!rowsToRemove.Contains(i))
                {
                    for (int j = 0; j < actualDictionaryValues.GetLength(1); j++)
                    {
                        newDictionaryValues[rowToWrite, j] = actualDictionaryValues[i, j];
                    }
                    rowToWrite++;
                }
            }
            dictionary.Value = new UAValue(newDictionaryValues);
        }

}
