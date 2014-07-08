// ----------------------------------------------------------------------------------
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

namespace Microsoft.Azure.Commands.BatchManager
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.WindowsAzure;
    using Microsoft.Azure.Management.Batch.Models;
    using Properties;

    [Cmdlet(VerbsCommon.New, "AzureBatchAccount"), OutputType(typeof(BatchAccountContext))]
    public class NewBatchAccountCommand : BatchCmdletBaseWithTags
    {
        private const string mamlRestName = "NewAccount";

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Batch service account to create.")]
        [Alias("Name")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The location where the account will be created.")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the resource group where the account will reside.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        // validate the name is available. No longer supported by the CSM. Let's see if they change their minds
        //[Parameter(Mandatory = false, HelpMessage = "Check if account name is available.")]
        //public SwitchParameter WhatIf { get; set; }

        public override void ExecuteCmdlet()
        {
            // check if we're just validating
            ResourceValidationMode? validationMode = null;
            //if (WhatIf.IsPresent)
            //{
            //    validationMode = ResourceValidationMode.NameAvailability;
            //    WriteVerboseWithTimestamp(Resources.NBA_NameAvailability, this.AccountName);
            //}
            //else
            //{
                // not checking availability so see if it already exists in our subscription. This won't help if
                // the account exists in a different subscription. Probably should always first call with nameAvailability
                // to make the global check but then look at the response code to figure out if the real create should
                // be re-issued in order actually create it. hmm...
            //}

            // use the group lookup to validate whether account already exists. We don't care about the returned
            // group name nor the exception
            WriteVerboseWithTimestamp(Resources.NBA_LookupAccount);
            if (BatchClient.GetGroupForAccountNoThrow(this.AccountName) != null)
            {
                throw new CloudException(Resources.NBA_AccountAlreadyExists);
            }

            WriteVerboseWithTimestamp(Resources.BeginMAMLCall, mamlRestName);
            var response = BatchClient.CreateAccount(this.ResourceGroupName, this.AccountName, new BatchAccountCreateParameters()
                {
                    Location = this.Location,
                    Tags = tagsDictionary,
                    ValidationMode = validationMode
                });

            WriteVerboseWithTimestamp(Resources.EndMAMLCall, mamlRestName);

            var context = BatchAccountContext.CrackAccountResourceToNewAccountContext(response.Resource);
            WriteObject(context);
        }
    }
}
