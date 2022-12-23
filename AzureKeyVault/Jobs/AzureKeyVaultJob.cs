﻿// Copyright 2022 Keyfactor
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using Keyfactor.Orchestrators.Extensions;
using Newtonsoft.Json;

namespace Keyfactor.Extensions.Orchestrator.AzureKeyVault
{
    public abstract class AzureKeyVaultJob<T> : IOrchestratorJobExtension
    {
        public string ExtensionName => AzureKeyVaultConstants.STORE_TYPE_NAME;

        internal protected virtual AzureClient AzClient { get; set; }
        internal protected virtual AkvProperties VaultProperties { get; set; }
        protected AzureAppServicesClient AppServicesClient { get; set; }

        public void InitializeStore(dynamic config)
        {
            VaultProperties = new AkvProperties();
            if (config.GetType().GetProperty("ClientMachine") != null)
                VaultProperties.SubscriptionId = config.ClientMachine;

            VaultProperties.TenantId = config.ServerUsername.Split()[0]; //username should contain "<tenantId guid> <app id guid> <object Id>"            
            VaultProperties.ApplicationId = config.ServerUsername.Split()[1];
            VaultProperties.ObjectId = config.ServerUsername.Split()[2];
            VaultProperties.ClientSecret = config.ServerPassword;

            if (config.GetType().GetProperty("CertificateStoreDetails") != null)
            {
                VaultProperties.StorePath = config.CertificateStoreDetails?.StorePath;
                dynamic properties = JsonConvert.DeserializeObject(config.CertificateStoreDetails.Properties.ToString());
                VaultProperties.ResourceGroupName = properties.ResourceGroupName;
                VaultProperties.VaultName = properties.VaultName;
                
                VaultProperties.AutoUpdateAppServiceBindings = (bool)properties.AutoUpdateBindings;
                // Make binding variable safe in case Keyfactor expects an older version of the extension
                if (properties.GetType().GetProperty("AutoUpdateBindings") != null)
                    VaultProperties.AutoUpdateAppServiceBindings = (bool)properties.AutoUpdateBindings;
            }
            AzClient ??= new AzureClient(VaultProperties);
            
            // If the store was configured to auto-update app service bindings, create a client to do so
            if (VaultProperties.AutoUpdateAppServiceBindings)
            {
                AppServicesClient ??= new AzureAppServicesClient(VaultProperties);
            }
        }
    }
}

