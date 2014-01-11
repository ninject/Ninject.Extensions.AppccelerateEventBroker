//--------------------------------------------------------------------------------------------------------------------
// <copyright file="IntegrationTests.cs" company="Ninject Project Contributors">
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
//--------------------------------------------------------------------------------------------------------------------

namespace Ninject.Extensions.AppccelerateEventBroker
{
    using System;
    using Appccelerate.EventBroker;
    using Appccelerate.EventBroker.Handlers;
    using FluentAssertions;
    using Xunit;

    public class IntegrationTests
    {
        private StandardKernel kernel;

        public IntegrationTests()
        {
            this.kernel = new StandardKernel();
        }

        [Fact]
        public void RegisterOnGlobalEventBroker()
        {
            const string EventBrokerName = "GlobalEventBroker2";
            this.kernel.AddGlobalEventBroker(EventBrokerName);
            this.kernel.Bind<Parent>().ToSelf().RegisterOnEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf().Named("FirstChild").RegisterOnEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf().Named("SecondChild");

            var parent = this.kernel.Get<Parent>();
            parent.FireSomeEvent();
                       
            parent.FirstChild.EventReceived.Should().BeTrue("Event was not received by child 1");
            parent.SecondChild.EventReceived.Should().BeFalse("Event was received by child 2");
        }

        [Fact]
        public void RegisterOnDefaultGlobalEventBroker()
        {
            const string EventBrokerName = "GlobalEventBroker2";
            this.kernel.AddGlobalEventBroker(EventBrokerName);
            this.kernel.Bind<Parent>().ToSelf().RegisterOnGlobalEventBroker();
            this.kernel.Bind<Child>().ToSelf().Named("FirstChild").RegisterOnGlobalEventBroker();
            this.kernel.Bind<Child>().ToSelf().Named("SecondChild").RegisterOnEventBroker(EventBrokerName);

            var parent = this.kernel.Get<Parent>();
            parent.FireSomeEvent();

            parent.FirstChild.EventReceived.Should().BeTrue("Event was not received by child 1");
            parent.SecondChild.EventReceived.Should().BeFalse("Event was received by child 2");
        }

        [Fact]
        public void RegisterOnLocalEventBroker()
        {
            const string EventBrokerName = "LocalEventBroker";
            this.kernel.Bind<Foo>().ToSelf();
            this.kernel.Bind<Parent>().ToSelf().RegisterOnEventBroker(EventBrokerName).OwnsEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf().Named("FirstChild").RegisterOnEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf().Named("SecondChild").RegisterOnEventBroker(EventBrokerName);

            var foo = this.kernel.Get<Foo>();
            foo.Parent1.FireSomeEvent();

            foo.Parent1.FirstChild.EventReceived.Should().BeTrue("Event was not received by parent1.child1");
            foo.Parent1.SecondChild.EventReceived.Should().BeTrue("Event was not received by parent1.child2");
            foo.Parent2.FirstChild.EventReceived.Should().BeFalse("Event was received by parent2.child1");
            foo.Parent2.SecondChild.EventReceived.Should().BeFalse("Event was received by parent2.child2");
        }

        public class Foo
        {
            public Foo(Parent parent1, Parent parent2)
            {
                this.Parent1 = parent1;
                this.Parent2 = parent2;
            }

            public Parent Parent1 { get; private set; }

            public Parent Parent2 { get; private set; }
        }

        public class Parent
        {
            public Parent(
                [Named("FirstChild")]Child firstChild,
                [Named("SecondChild")]Child secondChild)
            {
                this.FirstChild = firstChild;
                this.SecondChild = secondChild;
            }

            [EventPublication("SomeEventTopic")]
            public event EventHandler SomeEvent;

            public Child FirstChild { get; private set; }

            public Child SecondChild { get; private set; }

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