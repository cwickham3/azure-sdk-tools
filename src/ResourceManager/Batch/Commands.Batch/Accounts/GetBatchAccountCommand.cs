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

namespace Microsoft.Azure.Commands.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.WindowsAzure;
    using Microsoft.Azure.Management.Batch.Models;
    using Properties;

    [Cmdlet(VerbsCommon.Get, "AzureBatchAccount"), OutputType(typeof(BatchAccountContext))]
    public class GetBatchAccountCommand : BatchCmdletBase
    {
        // list all accounts in a subscription or in the specified resource group
        internal const string ParameterSetMultipleAccounts = "List accounts";

        // if account name is specified, list just that account
        internal const string ParameterSetSingleAccount = "Get a single account";

        // get account and resource name from BactchAccountContext object
        internal const string ParameterSetContext = "Use Context";
        
        private const string mamlRestName = "Get";

        [Alias("Name")]
        [Parameter(ParameterSetName = ParameterSetSingleAccount, Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Batch service account name to query.")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Parameter(ParameterSetName = ParameterSetSingleAccount, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group of the account being queried.")]
        //[Parameter(ParameterSetName = ParameterSetMultipleAccounts, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group of the account being queried.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        // Keeping for InputObject
        //[Parameter(ParameterSetName = ParameterSetContext, Mandatory = true, ValueFromPipeline = true,
        //    HelpMessage = "An existing context that identifies the account to use for the key query.")]
        //[ValidateNotNull]
        //public BatchAccountContext InputObject { get; set; }

        public override void ExecuteCmdlet()
        {
            string accountName = this.AccountName;
            string resourceGroupName = this.ResourceGroupName;

            //if (Context != null)
            //{
            //    accountName = Context.AccountName;
            //    resourceGroupName = Context.ResourceGroupName;
            //    context = Context;
            //}

            if (string.IsNullOrEmpty(accountName))
            {
                // no account name so we're doing some sort of list. If no resource group, then list all accounts under the
                // subscription otherwise all accounts in the resource group.
                if (resourceGroupName == null)
                {
                    WriteVerboseWithTimestamp(Resources.GBA_AllAccounts);
                }
                else
                {
                    WriteVerboseWithTimestamp(Resources.GBA_ResGroupAccounts, resourceGroupName);
                }

                WriteVerboseWithTimestamp(Resources.BeginMAMLCall, mamlRestName);
                var response = BatchClient.ListAccounts(new AccountListParameters { ResourceGroupName = resourceGroupName });
                WriteVerboseWithTimestamp(Resources.EndMAMLCall, mamlRestName);

                foreach (AccountResource resource in response.Accounts)
                {
                    var context = BatchAccountContext.CrackAccountResourceToNewAccountContext(resource);
                    WriteObject(context);
                }

                var nextLink = response.NextLink;

                while (nextLink != null)
                {
                    WriteVerboseWithTimestamp(Resources.BeginMAMLCall, mamlRestName);
                    response = BatchClient.ListNextAccounts(nextLink);
                    WriteVerboseWithTimestamp(Resources.EndMAMLCall, mamlRestName);

                    foreach (AccountResource resource in response.Accounts)
                    {
                        var context = BatchAccountContext.CrackAccountResourceToNewAccountContext(resource);
                        WriteObject(context);
                    }

                    nextLink = response.NextLink;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(resourceGroupName))
                {
                    // use resource mgr to see if account exists and then use resource group name to do the actual lookup
                    WriteVerboseWithTimestamp(Resources.ResGroupLookup, accountName);
                    resourceGroupName = BatchClient.GetGroupForAccount(accountName);
                }

                WriteVerboseWithTimestamp(Resources.BeginMAMLCall, mamlRestName);
                var response = BatchClient.GetAccount(resourceGroupName, accountName);
                WriteVerboseWithTimestamp(Resources.EndMAMLCall, mamlRestName);

                var context = BatchAccountContext.CrackAccountResourceToNewAccountContext(response.Resource);
                WriteObject(context);
            }
        }
    }
}
