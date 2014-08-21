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
    public class VMNetworkOperationsTest
    {
        private const string baseURI = "/VMNetworks";

        private const string vNetName = "VNet01";
        private const string vNetDescription = "VNet01 - Description";

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldCreateOneVMNetwork()
        {
            Guid VNetLogicalNetworkId = Guid.NewGuid();
            Guid StampId= Guid.NewGuid();

            var mockChannel = new MockRequestChannel();

            var vmNetworkToCreate = new VMNetwork()
            {
                Name = vNetName,
                Description = vNetDescription,
                LogicalNetworkId = VNetLogicalNetworkId,
                StampId = StampId,            
            };

            var vmNetworkToReturn = new VMNetwork()
            {
                Name = vNetName,
                Description = vNetDescription,
                LogicalNetworkId = VNetLogicalNetworkId,
                StampId = StampId,            
            };

            mockChannel.AddReturnObject(vmNetworkToReturn, new WebHeaderCollection { "x-ms-request-id:" + Guid.NewGuid() });

            Guid? jobOut;
            var VMNetworkOperations = new VMNetworkOperations(new WebClientFactory(new Subscription(), mockChannel));
            var createdVMNetwork = VMNetworkOperations.Create(vmNetworkToCreate, out jobOut);

            Assert.IsNotNull(createdVMNetwork);
            Assert.IsInstanceOfType(createdVMNetwork, typeof(VMNetwork));
            Assert.AreEqual(vmNetworkToReturn.Name, vmNetworkToCreate.Name);
            Assert.AreEqual(vmNetworkToReturn.Description, vmNetworkToCreate.Description);
            Assert.AreEqual(vmNetworkToReturn.LogicalNetworkId, vmNetworkToCreate.LogicalNetworkId);
            Assert.AreEqual(vmNetworkToReturn.StampId, vmNetworkToCreate.StampId);

            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(1, requestList.Count);
            Assert.AreEqual(HttpMethod.Post.ToString(), requestList[0].Item1.Method);

            // Check the URI
            Assert.AreEqual(baseURI, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldReturnOneVMNetworkByID()
        {
            var mockChannel = new MockRequestChannel();

            var vmNetworkToReturn = new VMNetwork()
            {
                Name = vNetName,
                Description = vNetDescription,
                LogicalNetworkId = Guid.Empty,
                StampId = Guid.Empty,
            };
            mockChannel.AddReturnObject(vmNetworkToReturn);

            var VMNetworkOperations = new VMNetworkOperations(new WebClientFactory(new Subscription(), mockChannel));
            var readVMNetwork = VMNetworkOperations.Read(Guid.Empty);
            Assert.AreEqual(Guid.Empty, readVMNetwork.ID);

            // Check the URI
            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(1, requestList.Count);
            Assert.AreEqual(baseURI, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldReturnOneVMNetworkByName()
        {
            var mockChannel = new MockRequestChannel();

            var vmNetworkToReturn = new VMNetwork()
            {
                Name = vNetName,
                Description = vNetDescription,
                LogicalNetworkId = Guid.Empty,
                StampId = Guid.Empty,
            };
            mockChannel.AddReturnObject(vmNetworkToReturn);

            var filter = new Dictionary<string, string>()
            {
                {"Name", vNetName}
            };
            var VMNetworkOperations = new VMNetworkOperations(new WebClientFactory(new Subscription(), mockChannel));
            var readVMNetwork = VMNetworkOperations.Read(filter);
            Assert.AreEqual(1, readVMNetwork.Count);
            Assert.AreEqual(vNetName, readVMNetwork.First().Name);

            // Check the URI
            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(1, requestList.Count);
            Assert.AreEqual(baseURI, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldReturnMultipleVMNetworks()
        {
            var mockChannel = new MockRequestChannel();
            var vmNetworkList = new List<object>
            {
                new VMNetwork { Name = vNetName, Description = vNetDescription },
                new VMNetwork { Name = vNetName, Description = vNetDescription }
            };
            mockChannel.AddReturnObject(vmNetworkList);

            var VMNetworkOperations = new VMNetworkOperations(new WebClientFactory(new Subscription(), mockChannel));
            var readVMNetwork = VMNetworkOperations.Read();

            Assert.AreEqual(2, readVMNetwork.Count);
            Assert.IsTrue(readVMNetwork.All(vmNetwork => vmNetwork.Name == vNetName));

            // Check the URI
            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(1, requestList.Count);
            Assert.AreEqual(baseURI, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldDeleteStaticVMNetwork()
        {
            var mockChannel = new MockRequestChannel();

            var existingVmNetwork = new VMNetwork()
            {
                Name = vNetName,
                Description = vNetDescription,
                LogicalNetworkId = Guid.Empty,
                StampId = Guid.Empty,
            };
            mockChannel.AddReturnObject(new Cloud() { StampId = Guid.NewGuid() });
            mockChannel.AddReturnObject(existingVmNetwork, new WebHeaderCollection { "x-ms-request-id:" + Guid.NewGuid() });

            Guid? jobOut;
            var VMNetworkOperations = new VMNetworkOperations(new WebClientFactory(new Subscription(), mockChannel));
            VMNetworkOperations.Delete(Guid.Empty, out jobOut);

            Assert.AreEqual(2, mockChannel.ClientRequests.Count);
            Assert.AreEqual(HttpMethod.Delete.ToString(), mockChannel.ClientRequests[1].Item1.Method);

            // Check the URI
            var requestURI = mockChannel.ClientRequests[1].Item1.Address.AbsolutePath;
            Assert.AreEqual("/Clouds", mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
            Assert.AreEqual(baseURI, mockChannel.ClientRequests[1].Item1.Address.AbsolutePath.Substring(1).Remove(requestURI.IndexOf('(') - 1));
        }
    }
}
