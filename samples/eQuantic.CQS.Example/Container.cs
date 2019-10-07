using System;
using System.Collections;
using System.Collections.Generic;
using Lamar;

namespace eQuantic.CQS.Example {
    public class Container : eQuantic.Core.Ioc.IContainer {
        private readonly IServiceContext serviceContext;
        public Container (IServiceContext serviceContext) {
            this.serviceContext = serviceContext;

        }
        public T Resolve<T> () {
            return serviceContext.GetInstance<T>();
        }

        public T Resolve<T> (string name) {
            return serviceContext.GetInstance<T>(name);
        }

        public object Resolve (Type type) {
            return serviceContext.GetInstance(type);
        }

        public object Resolve (string name, Type type) {
            return serviceContext.GetInstance(type, name);
        }

        public IEnumerable ResolveAll (Type type) {
            return serviceContext.GetAllInstances(type);
        }

        public IEnumerable<T> ResolveAll<T> () {
            return serviceContext.GetAllInstances<T>();
        }
    }
}