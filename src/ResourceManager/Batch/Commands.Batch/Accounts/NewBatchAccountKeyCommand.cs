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
    using Microsoft.Azure.Management.Batch.Models;

    [Cmdlet(VerbsCommon.New, "AzureBatchAccountKey"), OutputType(typeof(BatchAccountContext))]
    public class RegenBatchAccountKeyCommand : BatchCmdletBase
    {
        internal const string ParameterSetContext = "Use Context";
        internal const string ParameterSetCmdLine = "Use Command Line";

        private const string mamlRestName = "NewKey";

        [Parameter(ParameterSetName = ParameterSetCmdLine, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Batch service account to regenerate the specified key for.")]
        [Alias("Name")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Parameter(ParameterSetName = ParameterSetCmdLine, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The resource group of the account.")]
        public string ResourceGroupName { get; set; }

        private AccountKeyType keyType;
        //[Parameter(ParameterSetName = ParameterSetContext, Mandatory = true, ValueFromPipeline = false,
        //    HelpMessage = "The type of key (primary or secondary) to regenerate.")]
        [Parameter(ParameterSetName = ParameterSetCmdLine, Mandatory = true, ValueFromPipeline = false, HelpMessage = "The type of key (primary or secondary) to regenerate.")]
        [ValidateSet("Primary", "Secondary")]
        public string KeyType
        {
            get { return keyType.ToString(); }
            set
            {
                if (value == "Primary")
                {
                    keyType = AccountKeyType.Primary;
                }
                else if (value == "Secondary")
                {
                    keyType = AccountKeyType.Secondary;
                }
            }
        }

        //[Parameter(ParameterSetName = ParameterSetContext, Mandatory = true, ValueFromPipeline = true, 
        //    HelpMessage = "An existing context that identifies the account to use for the key regenration.")]
        //[ValidateNotNull]
        //public BatchAccountContext Context { get; set; }

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

            if (string.IsNullOrEmpty(resGroupName))
            {
                // use resource mgr to see if account exists and then use resource group name to do the actual lookup
                WriteVerboseWithTimestamp(Properties.Resources.ResGroupLookup, accountName);
                resGroupName = BatchClient.GetGroupForAccount(accountName);
            }

            // build a new context to put the keys into
            var getResponse = BatchClient.GetAccount(resGroupName, accountName);

            var context = BatchAccountContext.CrackAccountResourceToNewAccountContext(getResponse.Resource);

            var regenResponse = BatchClient.RegenerateKeys(resGroupName, accountName, new BatchAccountRegenerateKeyParameters
                {
                    KeyName = this.keyType
                });
            
            context.PrimaryAccountKey = regenResponse.PrimaryKey;
            context.SecondaryAccountKey = regenResponse.SecondaryKey;
            WriteObject(context);
        }
    }
}
