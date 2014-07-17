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
    using Microsoft.Azure.Management.Batch.Models;
    using Properties;

    [Cmdlet(VerbsCommon.Get, "AzureBatchAccountKeys"), OutputType(typeof(BatchAccountContext))]
    public class GetBatchAccountKeysCommand : BatchCmdletBase
    {
        internal const string ParameterSetContext = "Use Context";
        internal const string ParameterSetNames = "Use Names";

        private const string mamlRestName = "GetKeys";

        [Parameter(ParameterSetName = ParameterSetNames, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Batch service account to query keys for.")]
        [Alias("Name")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Parameter(ParameterSetName = ParameterSetNames, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group of the account.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        //[Parameter(ParameterSetName = ParameterSetContext, Mandatory = true, ValueFromPipeline = true, 
        //    HelpMessage = "An existing context that identifies the account to use for the key query.")]
        //[ValidateNotNull]
        //public BatchAccountContext Context { get; set; }

        /// <summary>
        /// Get the keys associated with the specified account. If only the account name is passed in, then
        /// look up its resource group and construct a new BatchAccountContext to hold everything.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            string accountName = this.AccountName;
            string resGroupName = this.ResourceGroupName;

            //if (Context != null)
            //{
            //    accountName = Context.AccountName;
            //    resGroupName = Context.ResourceGroupName;
            //    context = Context;
            //}

            WriteVerboseWithTimestamp(Resources.GBAK_GettingKeys, accountName);

            if (string.IsNullOrEmpty(resGroupName))
            {
                // use resource mgr to see if account exists and then use resource group name to do the actual lookup
                WriteVerboseWithTimestamp(Resources.ResGroupLookup, accountName);
                resGroupName = BatchClient.GetGroupForAccount(accountName);
            }

            var getResponse = BatchClient.GetAccount(resGroupName, accountName);
            var context = BatchAccountContext.CrackAccountResourceToNewAccountContext(getResponse.Resource);
            
            WriteVerboseWithTimestamp(Resources.BeginMAMLCall, mamlRestName);

            var keysResponse = BatchClient.ListKeys(resGroupName, accountName);
            
            WriteVerboseWithTimestamp(Resources.EndMAMLCall, mamlRestName);
            
            context.PrimaryAccountKey = keysResponse.PrimaryKey;
            context.SecondaryAccountKey = keysResponse.SecondaryKey;

            WriteObject(context);
        }
    }
}
