﻿// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.SqlDatabase.Servers.Cmdlet
{
    using System;
    using System.Management.Automation;
    using System.ServiceModel;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.SqlDatabase;
    using Microsoft.WindowsAzure.Management.SqlDatabase.Model;

    [Cmdlet(VerbsCommon.Set, "AzureSqlDatabasePassword")]
    public class SetAzureSqlDatabasePassword : SqlDatabaseManagementCmdletBase
    {
        public SetAzureSqlDatabasePassword()
        {
        }

        public SetAzureSqlDatabasePassword(ISqlAzureManagement channel)
        {
            this.Channel = channel;
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ServerName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string NewPassword
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                this.SetSqlAzurePasswordProcess();
            }           
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(this.ProcessExceptionDetails(ex), string.Empty, ErrorCategory.CloseError, null));
            }           
        }

        private void SetSqlAzurePasswordProcess()
        {
            try
            {
                using (new OperationContextScope((IContextChannel)Channel))
                {
                    this.RetryCall(s => this.Channel.SetPassword(s, this.ServerName, this.NewPassword));
                    Operation operation = WaitForSqlAzureOperation();
                    SqlDatabaseOperationContext context = new SqlDatabaseOperationContext()
                    {
                        OperationDescription = CommandRuntime.ToString(),
                        OperationId = operation.OperationTrackingId,
                        OperationStatus = operation.Status
                    };
                    WriteObject(context, true);
                }  
            }
            catch (CommunicationException ex)
            {
                this.WriteErrorDetails(ex);                
            }
        }
    }
}
