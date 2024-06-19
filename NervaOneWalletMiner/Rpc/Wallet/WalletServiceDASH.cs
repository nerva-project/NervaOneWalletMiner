using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Wallet
{
    internal class WalletServiceDASH : IWalletService
    {
        private static int _id = 0;

        #region Open Wallet
        public async Task<OpenWalletResponse> OpenWallet(RpcBase rpc, OpenWalletRequest requestObj)
        {
            OpenWalletResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["filename"] = requestObj.WalletName,
                    ["load_on_startup"] = requestObj.LoadOnStartup
                };

                var requestJson = new JObject
                {
                    ["id"] = _id++,
                    ["method"] = "loadwallet",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                if (httpResponse.IsSuccessStatusCode)
                {
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonXNV.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        // Just set error to false
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DAS.WOPW", ex);
            }

            return responseObj;
        }
        #endregion // Open Wallet

        #region Close Wallet
        public async Task<CloseWalletResponse> CloseWallet(RpcBase rpc, CloseWalletRequest requestObj)
        {
            CloseWalletResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["wallet_name"] = requestObj.WalletName,
                    ["load_on_startup"] = false

                };

                var requestJson = new JObject
                {
                    ["id"] = _id++,
                    ["method"] = "unloadwallet",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                if (httpResponse.IsSuccessStatusCode)
                {
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonXNV.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        // Just set error to false
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DAS.WCLW", ex);
            }

            return responseObj;
        }
        #endregion // Close Wallet

        #region Create Wallet
        public async Task<CreateWalletResponse> CreateWallet(RpcBase rpc, CreateWalletRequest requestObj)
        {
            CreateWalletResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["wallet_name"] = requestObj.WalletName,
                    ["passphrase"] = requestObj.Password
                };

                var requestJson = new JObject
                {
                    ["id"] = _id++,
                    ["method"] = "createwallet",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                if (httpResponse.IsSuccessStatusCode)
                {
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonXNV.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DAS.WCRW", ex);
            }

            return responseObj;
        }
        #endregion // Create Wallet

        public Task<CreateAccountResponse> CreateAccount(RpcBase rpc, CreateAccountRequest requestObj)
        {
            // getnewaddress
            throw new NotImplementedException();
        }

        public Task<LabelAccountResponse> LabelAccount(RpcBase rpc, LabelAccountRequest requestObj)
        {
            // setlabel
            throw new NotImplementedException();
        }

        public Task<SaveWalletResponse> SaveWallet(RpcBase rpc, SaveWalletRequest requestObj)
        {
            // TODO: If not needed, add coin specific flag indicating if wallet needs to be saved
            throw new NotImplementedException();
        }

        public Task<RestoreFromSeedResponse> RestoreFromSeed(RpcBase rpc, RestoreFromSeedRequest requestObj)
        {
            // TODO: Figure out how restoring works and make it work or change it so it's coin specific
            throw new NotImplementedException();
        }

        public Task<RestoreFromKeysResponse> RestoreFromKeys(RpcBase rpc, RestoreFromKeysRequest requestObj)
        {
            // importelectrumwallet
            // importwalle
            throw new NotImplementedException();
        }

        public Task<TransferResponse> Transfer(RpcBase rpc, TransferRequest requestObj)
        {
            // sendtoaddress
            throw new NotImplementedException();
        }

        public Task<TransferResponse> TransferSplit(RpcBase rpc, TransferRequest requestObj)
        {
            // Not supported
            throw new NotImplementedException();
        }

        public Task<RescanSpentResponse> RescanSpent(RpcBase rpc, RescanSpentRequest requestObj)
        {
            // scantxoutset
            throw new NotImplementedException();
        }

        public Task<RescanBlockchainResponse> RescanBlockchain(RpcBase rpc, RescanBlockchainRequest requestObj)
        {
            // rescanblockchain
            throw new NotImplementedException();
        }

        public Task<MakeIntegratedAddressResponse> MakeIntegratedAddress(RpcBase rpc, MakeIntegratedAddressRequest requestObj)
        {
            // TODO: Add coin specific flag and show this option, only if coin supports it
            throw new NotImplementedException();
        }

        public Task<GetAccountsResponse> GetAccounts(RpcBase rpc, GetAccountsRequest requestObj)
        {
            // listaddressbalances
            // getaddressinfo
            // getbalances
            throw new NotImplementedException();
        }

        public Task<GetTransfersResponse> GetTransfers(RpcBase rpc, GetTransfersRequest requestObj)
        {
            // listsinceblock
            // listtransactions
            throw new NotImplementedException();
        }

        public Task<GetTransferByTxIdResponse> GetTransferByTxId(RpcBase rpc, GetTranserByTxIdRequest requestObj)
        {
            // gettransaction
            throw new NotImplementedException();
        }

        public Task<GetHeightResponse> GetHeight(RpcBase rpc, GetHeightRequest requestObj)
        {
            // TODO: Used to show wallet height in the status bar. Try to get it some way. If it does not apply, change how this works
            throw new NotImplementedException();
        }

        public Task<QueryKeyResponse> QueryKey(RpcBase rpc, QueryKeyRequest requestObj)
        {
            // // DumpWallet
            throw new NotImplementedException();
        }
    }
}