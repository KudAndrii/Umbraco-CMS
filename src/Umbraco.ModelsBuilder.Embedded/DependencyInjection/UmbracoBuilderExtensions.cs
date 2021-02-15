using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Cms.ModelsBuilder.Embedded;
using Umbraco.Cms.ModelsBuilder.Embedded.Building;
using Umbraco.Cms.Web.Common.ModelBinders;

/*
 * OVERVIEW:
 *
 * The CSharpCompiler is responsible for the actual compilation of razor at runtime.
 * It creates a CSharpCompilation instance to do the compilation. This is where DLL references
 * are applied. However, the way this works is not flexible for dynamic assemblies since the references
 * are only discovered and loaded once before the first compilation occurs. This is done here:
 * https://github.com/dotnet/aspnetcore/blob/114f0f6d1ef1d777fb93d90c87ac506027c55ea0/src/Mvc/Mvc.Razor.RuntimeCompilation/src/CSharpCompiler.cs#L79
 * The CSharpCompiler is internal and cannot be replaced or extended, however it's references come from:
 * RazorReferenceManager. Unfortunately this is also internal and cannot be replaced, though it can be extended
 * using MvcRazorRuntimeCompilationOptions, except this is the place where references are only loaded once which
 * is done with a LazyInitializer. See https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RazorReferenceManager.cs#L35.
 *
 * The way that RazorReferenceManager works is by resolving references from the ApplicationPartsManager - either by
 * an application part that is specifically an ICompilationReferencesProvider or an AssemblyPart. So to fulfill this
 * requirement, we add the MB assembly to the assembly parts manager within the PureLiveModelFactory when the assembly
 * is (re)generated. But due to the above restrictions, when re-generating, this will have no effect since the references
 * have already been resolved with the LazyInitializer in the RazorReferenceManager. There is a known public API
 * where you can add reference paths to the runtime razor compiler via it's IOptions: MvcRazorRuntimeCompilationOptions
 * however this falls short too because those references are just loaded via the RazorReferenceManager and lazy initialized.
 *
 * The services that can be replaced are: IViewCompilerProvider (default is the internal RuntimeViewCompilerProvider) and
 * IViewCompiler (default is the internal RuntimeViewCompiler). There is one specific public extension point that I was
 * hoping would solve all of the problems which was IMetadataReferenceFeature (implemented by LazyMetadataReferenceFeature
 * which uses RazorReferencesManager) which is a razor feature that you can add
 * to the RazorProjectEngine. It is used to resolve roslyn references and by default is backed by RazorReferencesManager.
 * Unfortunately, this service is not used by the CSharpCompiler, it seems to only be used by some tag helper compilations.
 *
 * There are caches at several levels, all of which are not publicly accessible APIs (apart from RazorViewEngine.ViewLookupCache
 * which is possible to clear by casting and then calling cache.Compact(100); but that doesn't get us far enough).
 *
 * For this to work, several caches must be cleared:
 * - RazorViewEngine.ViewLookupCache
 * - RazorReferencesManager._compilationReferences
 * - RazorPageActivator._activationInfo (though this one may be optional)
 * - RuntimeViewCompiler._cache
 *
 * What are our options?
 *
 * a) We can copy a ton of code into our application: CSharpCompiler, RuntimeViewCompilerProvider, RuntimeViewCompiler and
 *    RazorReferenceManager (probably more depending on the extent of Internal references).
 * b) We can use reflection to try to access all of the above resources and try to forcefully clear caches and reset initialization flags.
 * c) We hack these replace-able services with our own implementations that wrap the default services. To do this
 *    requires re-resolving the original services from a pre-built DI container. In effect this re-creates these
 *    services from scratch which means there is no caches.
 *
 * ... Option C works, we will use that but need to verify how this affects memory since ideally the old services will be GC'd.
 *
 * Option C, how its done:
 * - Before we add our custom razor services to the container, we make a copy of the services collection which is the snapshot of registered services
 *   with razor defaults before ours are added.
 * - We replace the default implementation of IRazorViewEngine with our own. This is a wrapping service that wraps the default RazorViewEngine instance.
 *   The ctor for this service takes in a Factory method to re-construct the default RazorViewEngine and all of it's dependency graph.
 * - When the PureLive models change, the Factory is invoked and the default razor services are all re-created, thus clearing their caches and the newly
 *   created instance is wrapped. The RazorViewEngine is the only service that needs to be replaced and wrapped for this to work because it's dependency
 *   graph includes all of the above mentioned services, all the way up to the RazorProjectEngine and it's LazyMetadataReferenceFeature.
 */

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the common Umbraco functionality
    /// </summary>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds umbraco's embedded model builder support
        /// </summary>
        public static IUmbracoBuilder AddModelsBuilder(this IUmbracoBuilder builder)
        {
            var umbServices = new UniqueServiceDescriptor(typeof(UmbracoServices), typeof(UmbracoServices), ServiceLifetime.Singleton);
            if (builder.Services.Contains(umbServices))
            {
                // if this ext method is called more than once just exit
                return builder;
            }

            builder.Services.Add(umbServices);

            builder.AddPureLiveRazorEngine();

            // TODO: I feel like we could just do builder.AddNotificationHandler<ModelsBuilderNotificationHandler>() and it
            // would automatically just register for all implemented INotificationHandler{T}?
            builder.AddNotificationHandler<UmbracoApplicationStarting, ModelsBuilderNotificationHandler>();
            builder.AddNotificationHandler<ServerVariablesParsing, ModelsBuilderNotificationHandler>();
            builder.AddNotificationHandler<ModelBindingError, ModelsBuilderNotificationHandler>();
            builder.AddNotificationHandler<UmbracoApplicationStarting, LiveModelsProvider>();
            builder.AddNotificationHandler<UmbracoRequestEnd, LiveModelsProvider>();
            builder.AddNotificationHandler<UmbracoApplicationStarting, OutOfDateModelsStatus>();
            builder.Services.AddSingleton<ModelsGenerator>();
            builder.Services.AddSingleton<LiveModelsProvider>();
            builder.Services.AddSingleton<OutOfDateModelsStatus>();
            builder.Services.AddSingleton<ModelsGenerationError>();

            builder.Services.AddSingleton<PureLiveModelFactory>();

            // This is what the community MB would replace, all of the above services are fine to be registered
            // even if the community MB is in place.
            builder.Services.AddSingleton<IPublishedModelFactory>(factory =>
            {
                ModelsBuilderSettings config = factory.GetRequiredService<IOptions<ModelsBuilderSettings>>().Value;
                if (config.ModelsMode == ModelsMode.PureLive)
                {
                    return factory.GetRequiredService<PureLiveModelFactory>();
                }
                else
                {
                    return factory.CreateDefaultPublishedModelFactory();
                }
            });

            return builder;
        }

        /// <summary>
        /// Can be called if using an external models builder to remove the embedded models builder controller features
        /// </summary>
        public static IUmbracoBuilder DisableModelsBuilderControllers(this IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<DisableModelsBuilderNotificationHandler>();
            return builder;
        }

        private static IUmbracoBuilder AddPureLiveRazorEngine(this IUmbracoBuilder builder)
        {
            // See notes in RefreshingRazorViewEngine for information on what this is doing.

            // copy the current collection, we need to use this later to rebuild a container
            // to re-create the razor compiler provider
            var initialCollection = new ServiceCollection
            {
                builder.Services
            };

            // Replace the default with our custom engine
            builder.Services.AddSingleton<IRazorViewEngine>(
                s => new RefreshingRazorViewEngine(
                        () =>
                        {
                            // re-create the original container so that a brand new IRazorPageActivator
                            // is produced, if we don't re-create the container then it will just return the same instance.
                            ServiceProvider recreatedServices = initialCollection.BuildServiceProvider();
                            return recreatedServices.GetRequiredService<IRazorViewEngine>();
                        }, s.GetRequiredService<PureLiveModelFactory>()));

            return builder;
        }
    }
}
