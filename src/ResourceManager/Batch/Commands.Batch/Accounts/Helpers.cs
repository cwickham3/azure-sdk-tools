namespace Microsoft.Azure.Commands.BatchManager
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.WindowsAzure;
    using Microsoft.Azure.Management.Batch.Models;
    using Microsoft.Azure.Commands.BatchManager;
    
    internal class Helpers
    {
        static internal Dictionary<string, string> TagsArrayToDictionary(string[] tagData)
        {
            if (tagData.Length % 2 != 0)
            {
                return null;
            }

            var tagDictionary = new Dictionary<string, string>();
            for (int i = 0; i < tagData.Length; i = i + 2)
            {
                tagDictionary.Add(tagData[i], tagData[i + 1]);
            }

            return tagDictionary;
        }

        static internal string[] TagsDictionaryToArray(IDictionary<string, string> tagDictionary)
        {
            var tagArray = new string[tagDictionary.Count * 2];
            int i = 0;

            foreach (var kvp in tagDictionary)
            {
                tagArray[i] = kvp.Key;
                tagArray[i+1] = kvp.Value;
                i = i + 2;
            }

            return tagArray;
        }
    }
}