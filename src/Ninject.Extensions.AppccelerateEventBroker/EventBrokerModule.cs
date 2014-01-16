//-------------------------------------------------------------------------------
// <copyright file="EventBrokerModule.cs" company="Ninject Project Contributors">
//   Copyright (c) 2009-2014 Ninject Project Contributors
//   
//   Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
//   You may not use this file except in compliance with one of the Licenses.
//   You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   or
//       http://www.microsoft.com/opensource/licenses.mspx
//   
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// <Authors>
//   Ivan Appert (iappert@gmail.com)
// </Authors>
//-------------------------------------------------------------------------------

namespace Ninject.Extensions.AppccelerateEventBroker
{
    using System;
    using Ninject.Extensions.ContextPreservation;
    using Ninject.Extensions.NamedScope;
    using Ninject.Modules;

    /// <summary>
    /// Module for the event broker extension.
    /// </summary>
    public class EventBrokerModule : NinjectModule
    {
        /// <summary>
        /// The name of the default global event broker
        /// </summary>
        public const string DefaultGlobalEventBrokerName = "GlobalEventBroker";

        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load()
        {
            this.Kernel.AddGlobalEventBroker(DefaultGlobalEventBrokerName);
        }

        /// <summary>
        /// Called after loading the modules. A module can verify here if all other required modules are loaded.
        /// </summary>
        public override void VerifyRequiredModulesAreLoaded()
        {
            if (!this.Kernel.HasModule(typeof(NamedScopeModule).FullName))
            {
                throw new InvalidOperationException("The EventBrokerModule requires NamedScopeModule!");
            }

            if (!this.Kernel.HasModule(typeof(ContextPreservationModule).FullName))
            {
                throw new InvalidOperationException("The EventBrokerModule requires ContextPreservationModule!");
            }
        }
    }
}