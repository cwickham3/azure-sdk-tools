﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System.Collections;
using Microsoft.Azure.Commands.Resources.Models;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Resources
{
    /// <summary>
    /// Filters resource groups.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureResourceGroup"), OutputType(typeof(List<PSResourceGroup>))]
    public class GetAzureResourceGroupCommand : ResourcesBaseCmdlet
    {
        [Alias("ResourceGroupName")]
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the resource group.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group tag.")]
        public Hashtable Tag { get; set; }
        
        public override void ExecuteCmdlet()
        {
            WriteObject(ResourcesClient.FilterResourceGroups(Name, Tag), true);
        }
    }
}