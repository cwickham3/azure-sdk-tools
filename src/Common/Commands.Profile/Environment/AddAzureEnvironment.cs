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

namespace Microsoft.WindowsAzure.Commands.Profile
{
    using Commands.Utilities.Common;
    using System.Management.Automation;
    using System.Security.Permissions;

    /// <summary>
    /// Adds a new Microsoft Azure environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureEnvironment"), OutputType(typeof(WindowsAzureEnvironment))]
    public class AddAzureEnvironmentCommand : CmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string PublishSettingsFileUrl { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string ServiceEndpoint { get; set; }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public string ManagementPortalUrl { get; set; }

        [Parameter(Position = 4, Mandatory = false, HelpMessage = "The storage endpoint")]
        public string StorageEndpoint { get; set; }

        [Parameter(Position = 5, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The URI for the Active Directory service for this environment")]
        public string ActiveDirectoryEndpoint { get; set; }

        [Parameter(Position = 6, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The cloud service endpoint")]
        public string ResourceManagerEndpoint { get; set; }

        [Parameter(Position = 7, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The public gallery endpoint")]
        public string GalleryEndpoint { get; set; }

        [Parameter(Position = 8, Mandatory = false, ValueFromPipelineByPropertyName = true, 
            HelpMessage = "Identifier of the target resource that is the recipient of the requested token.")]
        public string ActiveDirectoryServiceEndpointResourceId { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            var newEnvironment = new WindowsAzureEnvironment
            {
                Name = Name,
                PublishSettingsFileUrl = PublishSettingsFileUrl,
                ServiceEndpoint = ServiceEndpoint,
                ResourceManagerEndpoint = ResourceManagerEndpoint,
                ManagementPortalUrl = ManagementPortalUrl,
                StorageEndpointSuffix = StorageEndpoint,
                ActiveDirectoryEndpoint = ActiveDirectoryEndpoint,
                ActiveDirectoryServiceEndpointResourceId = ActiveDirectoryServiceEndpointResourceId,
                ActiveDirectoryCommonTenantId = "Common",
                GalleryEndpoint = GalleryEndpoint
            };

            WindowsAzureProfile.Instance.AddEnvironment(newEnvironment);
            WriteObject(newEnvironment);
        }
    }
}