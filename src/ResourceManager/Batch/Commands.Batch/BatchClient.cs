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
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Microsoft.Azure.Management.Resources;
    using Microsoft.Azure.Management.Batch;
    using Microsoft.Azure.Management.Batch.Models;
    using Properties;

    public class BatchClient
    {
        public IBatchManagementClient BatchManagementClient{ get; private set; }

        public IResourceManagementClient ResourceManagementClient { get; private set; }

        private static string batchProvider = "Microsoft.Batch";
        private static string accountObject = "batchAccounts";
        private static string accountSearch = batchProvider + "/" + accountObject;


        /// <summary>
        /// Creates new BatchClient instance
        /// </summary>
        /// <param name="batchManagementClient">The IBatchManagementClient instance</param>
        public BatchClient(IBatchManagementClient batchManagementClient, IResourceManagementClient resourceManagementClient)
        {
            BatchManagementClient = batchManagementClient;
            ResourceManagementClient = resourceManagementClient;
        }

        /// <summary>
        /// Creates new ResourceManagementClient
        /// </summary>
        /// <param name="subscription">Subscription containing a batch account to manipulate</param>
        public BatchClient(WindowsAzureSubscription subscription)
            : this(subscription.CreateClientFromResourceManagerEndpoint<BatchManagementClient>(),
            subscription.CreateClientFromResourceManagerEndpoint<ResourceManagementClient>())
        {
        }

        #region Account verbs
        /// <summary>
        /// Creates a new Batch account
        /// </summary>
        /// <param name="resourceGroupName">The name of the resource group in which to create the account</param>
        /// <param name="accountName">The account name</param>
        /// <param name="parameters">Additional parameters that are associated with the creation of the account</param>
        /// <returns>The status of create operation</returns>
        public BatchAccountCreateResponse CreateAccount(string resourceGroupName, string accountName, BatchAccountCreateParameters parameters)
        {
            return BatchManagementClient.Accounts.Create(resourceGroupName, accountName, parameters);
        }

        /// <summary>
        /// Updates an existing Batch account
        /// </summary>
        /// <param name="resourceGroupName">The name of the resource group in which to create the account</param>
        /// <param name="accountName">The account name</param>
        /// <param name="parameters">Additional parameters that are associated with the update of the account</param>
        /// <returns>The status of update operation</returns>
        public BatchAccountUpdateResponse UpdateAccount(string resourceGroupName, string accountName, BatchAccountUpdateParameters parameters)
        {
            return BatchManagementClient.Accounts.Update(resourceGroupName, accountName, parameters);
        }

        /// <summary>
        /// Get details about the Batch account
        /// </summary>
        /// <param name="resourceGroupName">The name of the resource group in which to create the account</param>
        /// <param name="accountName">The account name</param>
        /// <returns>The status of get operation</returns>
        public BatchAccountGetResponse GetAccount(string resourceGroupName, string accountName)
        {
            return BatchManagementClient.Accounts.Get(resourceGroupName, accountName);
        }
        
        /// <summary>
        /// Gets the keys associated with the Batch account
        /// </summary>
        /// <param name="resourceGroupName">The name of the resource group in which to create the account</param>
        /// <param name="accountName">The account name</param>
        /// <returns>The status of get keys operation</returns>
        public BatchAccountListKeyResponse ListKeys(string resourceGroupName, string accountName)
        {
            return BatchManagementClient.Accounts.ListKeys(resourceGroupName, accountName);
        }

        /// <summary>
        /// Lists all accounts in a subscription or in a resource group if its name is specified
        /// </summary>
        /// <param name="listParameters">Additional parameters that are associated with the listing of accounts</param>
        /// <returns>The status of list operation</returns>
        public BatchAccountListResponse ListAccounts(AccountListParameters listParameters)
        {
            return BatchManagementClient.Accounts.List(listParameters);
        }

        /// <summary>
        /// Lists all accounts in a subscription or in a resource group if its name is specified
        /// </summary>
        /// <param name="listParameters">Additional parameters that are associated with the listing of accounts</param>
        /// <returns>The status of list operation</returns>
        public BatchAccountListResponse ListNextAccounts(string NextLink)
        {
            return BatchManagementClient.Accounts.ListNext(NextLink);
        }

        /// <summary>
        /// Generates new key for the Batch account
        /// </summary>
        /// <param name="resourceGroupName">The name of the resource group in which to create the account</param>
        /// <param name="accountName">The account name</param>
        /// <param name="parameters">Additional parameters that are associated with regenerating a key of the account</param>
        /// <returns>The status of regenerate key operation</returns>
        public BatchAccountRegenerateKeyResponse RegenerateKeys(string resourceGroupName, string accountName, BatchAccountRegenerateKeyParameters parameters)
        {
            return BatchManagementClient.Accounts.RegenerateKey(resourceGroupName, accountName, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceGroupName">The name of the resource group in which to create the account</param>
        /// <param name="accountName">The account name</param>
        /// <returns>The status of delete account operation</returns>
        public OperationResponse DeleteAccount(string resourceGroupName, string accountName)
        {
            return BatchManagementClient.Accounts.Delete(resourceGroupName, accountName);
        }
        #endregion

        internal string GetGroupForAccountNoThrow(string accountName)
        {
            var response = ResourceManagementClient.Resources.List(new Management.Resources.Models.ResourceListParameters()
            {
                ResourceType = accountSearch
            });

            string groupName = null;

            foreach (var res in response.Resources)
            {
                if (res.Name == accountName)
                {
                    groupName = ExtractResourceGroupName(res.Id);
                }
            }

            return groupName;
        }

        internal string GetGroupForAccount(string accountName)
        {
            var groupName = GetGroupForAccountNoThrow(accountName);
            if (groupName == null)
            {
                throw new Exception(Resources.ResourceNotFound);
            }

            return groupName;
        }

        private string ExtractResourceGroupName(string id)
        {
            var idParts = id.Split('/');
            if (idParts.Length < 4)
            {
                throw new CloudException(String.Format(Resources.MissingResGroupName, id));
            }

            return idParts[4];
        }
    }
}
