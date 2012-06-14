using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Should;
using WebApiContrib.IoC.Mef;
using WebApiContrib.IoC.Mef.Tests.Parts;
using Xunit;

namespace WebApiContrib.IoC.Mef.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void MefDependencyResolver_Resolves_Registered_ContactRepository_Test()
        {
            var resolver = MefDependencyResolver.CreateWithDefaultConventions(new[] { Assembly.GetAssembly(typeof(IContactRepository)) });

            var instance = resolver.GetService(typeof(IContactRepository));

            instance.ShouldNotBeNull();
        }

        [Fact]
        public void MefDependencyResolver_DoesNot_Resolve_NonRegistered_ContactRepository_Test()
        {
            var resolver = MefDependencyResolver.CreateWithDefaultConventions(new Assembly[0]);

            var instance = resolver.GetService(typeof(IContactRepository));

            instance.ShouldBeNull();
        }

        [Fact]
        public void MefDependencyResolver_Resolves_Registered_ContactRepository_ThroughHost_Test()
        {
            var config = new HttpConfiguration();
            var resolver = MefDependencyResolver.CreateWithDefaultConventions(new[] { Assembly.GetAssembly(typeof(IContactRepository)) });
            config.DependencyResolver = resolver;

            var server = new HttpServer(config);
            var client = new HttpClient(server);

            client.GetAsync("http://anything/api/contacts").ContinueWith(task =>
            {
                var response = task.Result;
                response.Content.ShouldNotBeNull();
            });
        }

        [Fact]
        public void MefDependencyResolver_In_HttpConfig_DoesNot_Resolve_PipelineType_But_Fallback_To_DefaultResolver_Test()
        {
            var config = new HttpConfiguration();
            var resolver = MefDependencyResolver.CreateWithDefaultConventions(new Assembly[0]);
            config.DependencyResolver = resolver;

            var instance = config.Services.GetService(typeof(IHttpActionSelector));

            instance.ShouldNotBeNull();
        }

        [Fact]
        public void MefDependencyResolver_DoesNot_Resolve_NonRegistered_ContactRepositories_Test()
        {
            var config = new HttpConfiguration();
            var resolver = MefDependencyResolver.CreateWithDefaultConventions(new Assembly[0]);
            config.DependencyResolver = resolver;

            var repositories = config.DependencyResolver.GetServices(typeof(IContactRepository));

            repositories.Count().ShouldEqual(0);
        }

        [Fact]
        public void MefDependencyResolver_Resolves_Registered_Both_Instaces_Of_IContactRepository()
        {
            var config = new HttpConfiguration();
            var resolver = MefDependencyResolver.CreateWithDefaultConventions(new[] { Assembly.GetAssembly(typeof(IContactRepository)) });
            config.DependencyResolver = resolver;

            var repositories = config.DependencyResolver.GetServices(typeof(IContactRepository));

            repositories.Count().ShouldEqual(2);
        }
    }
}
