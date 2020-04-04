//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using Nethermind.Blockchain.Filters;
using Nethermind.Core;
using Nethermind.Facade;
using Nethermind.JsonRpc.Modules;
using Nethermind.JsonRpc.Modules.Eth;
using Nethermind.Logging;
using Nethermind.State;
using Nethermind.Store.Bloom;
using Nethermind.Wallet;
using Newtonsoft.Json;
using NSubstitute;

namespace Nethermind.JsonRpc.Test.Modules
{
    public class TestRpcBlockchain : TestBlockchain
    {
        public IEthModule EthModule { get; private set; }
        public IBlockchainBridge Bridge { get; private set; }

        protected TestRpcBlockchain(SealEngineType sealEngineType)
            : base(sealEngineType)
        {
        }

        public static Builder ForTest(SealEngineType sealEngineType)
        {
            return new Builder(sealEngineType);
        }

        public class Builder
        {
            public Builder(SealEngineType sealEngineType)
            {
                _blockchain = new TestRpcBlockchain(sealEngineType);
            }
            
            private TestRpcBlockchain _blockchain;
            
            public Builder WithBlockchainBridge(IBlockchainBridge blockchainBridge)
            {
                _blockchain.Bridge = blockchainBridge;
                return this;
            }
            
            public TestRpcBlockchain Build()
            {
                return (TestRpcBlockchain)_blockchain.Build();
            }
        }

        protected override TestBlockchain Build()
        {
            base.Build();
            IStateReader stateReader = new StateReader(StateDb, CodeDb, LimboLogs.Instance);
            IFilterStore filterStore = new FilterStore();
            IFilterManager filterManager = new FilterManager(filterStore, BlockProcessor, TxPool, LimboLogs.Instance);
            Bridge ??= new BlockchainBridge(stateReader, StateProvider, StorageProvider, BlockTree, TxPool, ReceiptStorage, filterStore, filterManager, NullWallet.Instance, TxProcessor, EthereumEcdsa, NullBloomStorage.Instance, LimboLogs.Instance);
            EthModule = new EthModule(new JsonRpcConfig(), LimboLogs.Instance, Bridge);
            return this;
        }

        public string TestEthRpc(string method, params string[] parameters)
        {
            return RpcTest.TestSerializedRequest(EthModuleFactory.Converters, EthModule, method, parameters);
        }

        public string TestSerializedRequest<T>(T module, string method, params string[] parameters) where T : class, IModule
        {
            return RpcTest.TestSerializedRequest(new JsonConverter[0], module, method, parameters);
        }
    }
}