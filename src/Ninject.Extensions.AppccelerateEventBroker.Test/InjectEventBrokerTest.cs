//-------------------------------------------------------------------------------
// <copyright file="InjectEventBrokerTest.cs" company="Ninject Project Contributors">
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
    using Appccelerate.EventBroker;
    using Appccelerate.EventBroker.Handlers;
    using FluentAssertions;
    using Xunit;

    public class InjectEventBrokerTest
    {
        private readonly StandardKernel kernel;

        public InjectEventBrokerTest()
        {
            this.kernel = new StandardKernel();
        }

        [Fact]
        public void InjectDefaultGlobalEventBroker()
        {
            this.kernel.Bind<ParentWithDefaultEventBroker>().ToSelf().RegisterOnGlobalEventBroker();
            this.kernel.Bind<Child>().ToSelf();

            var parent = this.kernel.Get<ParentWithDefaultEventBroker>();
            parent.FireSomeEvent();

            parent.FirstChild.EventReceived.Should().BeTrue("Event was not received by child 1");
        }

        [Fact]
        public void InjectNamedGlobalEventBroker()
        {
            const string EventBrokerName = "EventBrokerName";
            this.kernel.AddGlobalEventBroker(EventBrokerName);
            this.kernel.Bind<Parent>().ToSelf().RegisterOnEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf();

            var parent = this.kernel.Get<Parent>();
            parent.FireSomeEvent();

            parent.FirstChild.EventReceived.Should().BeTrue("Event was not received by child 1");
        }

        [Fact]
        public void InjectLocalEventBroker()
        {
            const string EventBrokerName = "EventBrokerName";
            this.kernel.Bind<Parent>().ToSelf().RegisterOnEventBroker(EventBrokerName).OwnsEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf();

            var parent = this.kernel.Get<Parent>();
            parent.FireSomeEvent();

            parent.FirstChild.EventReceived.Should().BeTrue("Event was not received by child 1");
        }

        public class ParentWithDefaultEventBroker : Parent
        {
            public ParentWithDefaultEventBroker(Child child, IEventBroker globalEventBroker) 
                : base(child, globalEventBroker)
            {
            }
        }
        
        public class Parent
        {
            public Parent(Child child, IEventBroker eventBrokerName)
            {
                this.FirstChild = child;
                eventBrokerName.Register(child);
            }

            [EventPublication("SomeEventTopic")]
            public event EventHandler SomeEvent;

            public Child FirstChild { get; private set; }

            public void FireSomeEvent()
            {
                if (this.SomeEvent != null)
                {
                    this.SomeEvent(this, EventArgs.Empty);
                }
            }
        }

        public class Child
        {
            public bool EventReceived { get; private set; }

            [EventSubscription("SomeEventTopic", typeof(OnPublisher))]
            public void HandleSomeEvent(object sender, EventArgs e)
            {
                this.EventReceived = true;
            }
        }
    }
}