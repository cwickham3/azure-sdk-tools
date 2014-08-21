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

namespace Microsoft.WindowsAzure.Commands.Test.WAPackIaaS.Operations
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Test.WAPackIaaS.Mocks;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.DataContract;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Operations;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.WebClient;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    [TestClass]
    public class CloudServiceOperationsTests
    {
        private const string baseURI = "/CloudServices";

        private const string cloudServiceName = "CloudService01";
        private const string cloudServiceLabel = "CloudService01-Label";

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldCreateOneCloudService()
        {
            var mockChannel = new MockRequestChannel();

            var cloudServiceToCreate = new CloudService { Name = cloudServiceName, Label = cloudServiceLabel };
            var cloudServiceToReturn = new CloudService
            {
                Name = cloudServiceName,
                Label = cloudServiceLabel,
            };
            mockChannel.AddReturnObject(cloudServiceToReturn, new WebHeaderCollection { "x-ms-request-id:" + Guid.NewGuid() });

            Guid? jobOut;
            var cloudServiceOperations = new CloudServiceOperations(new WebClientFactory(new Subscription(), mockChannel));
            var createdCloudService = cloudServiceOperations.Create(cloudServiceToCreate, out jobOut);

            Assert.IsNotNull(createdCloudService);
            Assert.IsInstanceOfType(createdCloudService, typeof(CloudService));
            Assert.AreEqual(cloudServiceToReturn.Name, createdCloudService.Name);
            Assert.AreEqual(cloudServiceToReturn.Label, createdCloudService.Label);

            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(1, requestList.Count);
            Assert.AreEqual(HttpMethod.Post.ToString(), requestList[0].Item1.Method);

            // Check the URI (for Azure consistency)
            Assert.AreEqual(baseURI, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldReturnOneCloudService()
        {
            var mockChannel = new MockRequestChannel();
            mockChannel.AddReturnObject(new CloudService { Name = cloudServiceName, Label = cloudServiceLabel });
            mockChannel.AddReturnObject(new CloudResource());

            var cloudServiceOperations = new CloudServiceOperations(new WebClientFactory(new Subscription(), mockChannel));
            Assert.AreEqual(1, cloudServiceOperations.Read().Count);

            // Check the URI (for Azure consistency)
            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(3, requestList.Count);
            Assert.AreEqual(baseURI, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldReturnOneCloudServiceByName()
        {
            var mockChannel = new MockRequestChannel();
            mockChannel.AddReturnObject(new CloudService { Name = cloudServiceName, Label = cloudServiceLabel });
            mockChannel.AddReturnObject(new CloudResource());

            var cloudServiceOperations = new CloudServiceOperations(new WebClientFactory(new Subscription(), mockChannel));
            Assert.AreEqual(cloudServiceName, cloudServiceOperations.Read(cloudServiceName).Name);

            // Check the URI (for Azure consistency)
            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(3, requestList.Count);
            Assert.AreEqual(baseURI + "/" + cloudServiceName, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldReturnMultipleCloudServices()
        {
            var mockChannel = new MockRequestChannel();
            var cldList = new List<object>
            {
                new CloudService { Name = cloudServiceName, Label = cloudServiceLabel },
                new CloudService { Name = cloudServiceName, Label = cloudServiceLabel }
            };
            mockChannel.AddReturnObject(cldList);
            mockChannel.AddReturnObject(new CloudResource()).AddReturnObject(new VMRole { Name = "VMRole01", Label = "VMRole01-Label" }).AddReturnObject(new VM {});
            mockChannel.AddReturnObject(new CloudResource()).AddReturnObject(new VMRole { Name = "VMRole01", Label = "VMRole01-Label" }).AddReturnObject(new VM {});

            var cloudServiceOperations = new CloudServiceOperations(new WebClientFactory(new Subscription(), mockChannel));
            var cloudServiceList = cloudServiceOperations.Read();

            Assert.AreEqual(2, cloudServiceList.Count);
            Assert.IsTrue(cloudServiceList.All(cloudService => cloudService.Name == cloudServiceName));

            // Check the URI (for Azure consistency)
            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(7, requestList.Count);
            Assert.AreEqual(baseURI, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldDeleteCloudService()
        {
            var mockChannel = new MockRequestChannel();
            mockChannel.AddReturnObject(new CloudService { Name = cloudServiceName, Label = cloudServiceLabel }, new WebHeaderCollection { "x-ms-request-id:" + Guid.NewGuid() });

            Guid? jobOut;
            var cloudServiceOperations = new CloudServiceOperations(new WebClientFactory(new Subscription(), mockChannel));
            cloudServiceOperations.Delete(cloudServiceName, out jobOut);

            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(1, requestList.Count);
            Assert.AreEqual(HttpMethod.Delete.ToString(), requestList[0].Item1.Method);

            // Check the URI (for Azure consistency)
            Assert.AreEqual(baseURI + "/" + cloudServiceName, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        [TestCategory("WAPackIaaS-Negative")]
        public void ShouldReturnEmptyOnNoResult()
        {
            var cloudServiceOperations = new CloudServiceOperations(new WebClientFactory(new Subscription(), MockRequestChannel.Create()));
            Assert.IsFalse(cloudServiceOperations.Read().Any());
        }
    }
}
