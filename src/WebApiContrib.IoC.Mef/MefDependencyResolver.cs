using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;

namespace WebApiContrib.IoC.Mef
{
    public class MefDependencyScope : IDependencyScope
    {
        readonly Export<CompositionContext> _compositionScope;

        public MefDependencyScope(Export<CompositionContext> compositionScope)
        {
            if (compositionScope == null)
                throw new ArgumentNullException("compositionScope");

            _compositionScope = compositionScope;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            object result;
            CompositionScope.TryGetExport(serviceType, null, out result);
            return result;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            return CompositionScope.GetExports(serviceType, null);
        }

        protected CompositionContext CompositionScope
        {
            get { return _compositionScope.Value; }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
                _compositionScope.Dispose();
        }
    }

    public class MefDependencyResolver : MefDependencyScope, IDependencyResolver
    {
        readonly ExportFactory<CompositionContext> _requestScopeFactory;

        public MefDependencyResolver(CompositionHost rootCompositionScope)
            : base(new Export<CompositionContext>(rootCompositionScope, rootCompositionScope.Dispose))
        {
            if (rootCompositionScope == null)
                throw new ArgumentNullException("rootCompositionScope");

            var factoryContract = new CompositionContract(typeof(ExportFactory<CompositionContext>), null, new Dictionary<string, object>
            {
                { "SharingBoundaryNames", new[] { "HttpRequest" } }
            });

            _requestScopeFactory = (ExportFactory<CompositionContext>) rootCompositionScope.GetExport(factoryContract);
        }

        public IDependencyScope BeginScope()
        {
            return new MefDependencyScope(_requestScopeFactory.CreateExport());
        }

        public static IDependencyResolver CreateWithDefaultConventions(Assembly[] appAssemblies)
        {
            var conventions = new ConventionBuilder();

            conventions.ForTypesDerivedFrom<IHttpController>()
                .Export();

            conventions.ForTypesMatching(t => t.Namespace != null && t.Namespace.EndsWith(".Parts"))
                .Export()
                .ExportInterfaces();

            var container = new ContainerConfiguration()
                .WithAssemblies(appAssemblies, conventions)
                .CreateContainer();

            return new MefDependencyResolver(container);
        }
    }
}
