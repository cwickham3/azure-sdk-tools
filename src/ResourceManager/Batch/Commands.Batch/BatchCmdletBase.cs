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

namespace Microsoft.Azure.Commands.Batch
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;
    using Newtonsoft.Json.Linq;

    public class BatchCmdletBase : CmdletWithSubscriptionBase
    {
        private BatchClient batchClient;

        public BatchClient BatchClient
        {
            get
            {
                if (batchClient == null)
                {
                    batchClient = new BatchClient(CurrentSubscription);
                }
                return batchClient;
            }

            set { batchClient = value; }
        }

        protected virtual void OnProcessRecord()
        {
            // Intentionally left blank
        }

        protected override void ProcessRecord()
        {
            try
            {
                Validate.ValidateInternetConnection();
                ExecuteCmdlet();
                OnProcessRecord();
            }
            catch (CloudException ex)
            {
                var updatedEx = ex;

                if (ex.Response != null && ex.Response.Content != null)
                {
                    var message = FindDetailedMessage(ex.Response.Content);

                    if (message != null)
                    {
                        updatedEx = new CloudException(message, ex.InnerException);
                    }
                }

                WriteExceptionError(updatedEx);
            }
            catch (Exception ex)
            {
                WriteExceptionError(ex);
            }
        }

        /// <summary>
        /// For now, the 2nd message KVP inside "details" contains useful info about the failure. Eventually, a code KVP
        /// will be added such that we can search on that directly.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        internal static string FindDetailedMessage(string content)
        {
            string message = null;

            if (JsonUtilities.IsJson(content))
            {
                var response = JObject.Parse(content);

                // check that we have a details section
                var detailsToken = response["details"];

                if (detailsToken != null)
                {
                    var details = detailsToken as JArray;
                    if (details != null && details.Count > 1)
                    {
                        // for now, 2nd entry in array is the one we're interested in. Need a better way of identifying the
                        // detailed error message
                        var dObj = detailsToken[1] as JObject;
                        var code = dObj.GetValue("code", StringComparison.CurrentCultureIgnoreCase);
                        if (code != null)
                        {
                            message = code.ToString() + ": ";
                        }

                        var detailedMsg = dObj.GetValue("message", StringComparison.CurrentCultureIgnoreCase);
                        if (detailedMsg != null)
                        {
                            message += detailedMsg.ToString();

                        }
                    }
                }
            }

            return message;
        }
    }

    public class BatchCmdletBaseWithTags : BatchCmdletBase
    {
        //
        // tags to be generically handled by common code. Placeholder for now
        //
        internal Dictionary<string, string> tagsDictionary;
        internal string[] tags;
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Set of optional key/value pairs to be associated with the account.")]
        [AllowEmptyCollection]
        public string[] Tags
        {
            get { return tags; }
            set
            {
                if (value.Length != 0)
                {
                    tagsDictionary = Helpers.TagsArrayToDictionary(value);
                    if (tagsDictionary == null)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(Properties.Resources.InvalidTagsString), "1001", ErrorCategory.InvalidArgument, value));
                    }

                    tags = value;
                }
            }
        }
    }
}