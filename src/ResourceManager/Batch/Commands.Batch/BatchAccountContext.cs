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
    using Microsoft.Azure.Management.Batch.Models;
    using Properties;

    /// <summary>
    /// very simple account context class for getting things started
    /// </summary>
    public class BatchAccountContext
    {
        public string Id { get; private set; }

        public string AccountEndpoint { get; private set; }

        public string PrimaryAccountKey { get; internal set; }

        public string SecondaryAccountKey { get; internal set; }

        public string AccountName { get; private set; }

        public string Location { get; private set; }

        public string ResourceGroupName { get; private set; }

        public string Subscription { get; private set; }

        public string State { get; private set; }

        public string TaskTenantUrl { get; private set; }

        public string[] Tags { get; private set; }

        internal BatchAccountContext() { }

        internal BatchAccountContext(string id, string subscription, string accountEndpoint, string resourceGroupName, string location, string state, string[] tags)
        {
        }

        internal BatchAccountContext(string accountEndpoint)
        {
            this.AccountEndpoint = accountEndpoint;
        }

        /// <summary>
        /// Take an AccountResource and turn it into a BatchAccountContext
        /// </summary>
        /// <param name="resource">Resource info returned by RP</param>
        /// <returns>Void</returns>
        internal void CrackAccountResourceToAccountContext(AccountResource resource)
        {
            var accountEndpoint = resource.Properties.AccountEndpoint;
            if (Uri.CheckHostName(accountEndpoint) != UriHostNameType.Dns)
            {
                throw new ArgumentException(String.Format(Resources.InvalidEndpointType, accountEndpoint), "AccountEndpoint");
            }

            this.Id = resource.Id;
            this.AccountEndpoint = accountEndpoint;
            this.Location = resource.Location;
            this.State = resource.Properties.ProvisioningState.ToString();
            this.Tags = Helpers.TagsDictionaryToArray(resource.Tags);

            // extract the host and strip off the account name for the TaskTenantUrl and AccountName
            var hostParts = accountEndpoint.Split('.');
            this.AccountName = hostParts[0];
            this.TaskTenantUrl = Uri.UriSchemeHttps + Uri.SchemeDelimiter + String.Join(".", hostParts, 1, hostParts.Length - 1);

            // get remaining fields from Id which looks like:
            // /subscriptions/4a06fe24-c197-4353-adc1-058d1a51924e/resourceGroups/clwtest/providers/Microsoft.Batch/batchAccounts/clw
            var idParts = resource.Id.Split('/');
            this.Subscription = idParts[2];
            this.ResourceGroupName = idParts[4];
        }

        /// <summary>
        /// Create a new BAC and fill it in
        /// </summary>
        /// <param name="resource">Resource info returned by RP</param>
        /// <returns>new instance of BatchAccountContext</returns>
        internal static BatchAccountContext CrackAccountResourceToNewAccountContext(AccountResource resource)
        {
            var baContext = new BatchAccountContext();
            baContext.CrackAccountResourceToAccountContext(resource);
            return baContext;
        }
    }
}
