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


namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System;
    using System.Management.Automation;
    using Model;
    using Model.PersistentVMModel;
    using Properties;

    [Cmdlet(VerbsCommon.Set, "AzureOSDisk"), OutputType(typeof(IPersistentVM))]
    public class SetAzureOSDiskCommand : VirtualMachineConfigurationCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Controls the platform caching behavior of data disk blob for read / write efficiency.")]
        [ValidateSet("ReadOnly", "ReadWrite", IgnoreCase = true)]
        public string HostCaching
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            var role = VM.GetInstance(); 

            if (role.OSVirtualHardDisk == null)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                            new InvalidOperationException(Resources.OSDiskNotDefinedForVM),
                            string.Empty,
                            ErrorCategory.InvalidData,
                            null));
            }

            OSVirtualHardDisk disk = role.OSVirtualHardDisk;
            disk.HostCaching = HostCaching;
            WriteObject(VM, true);
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}