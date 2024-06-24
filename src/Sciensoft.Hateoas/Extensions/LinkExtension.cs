using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sciensoft.Hateoas.Filters;
using Sciensoft.Hateoas.Providers;
using Sciensoft.Hateoas.Repositories;
using System;

namespace Sciensoft.Hateoas.Extensions
{
	/// <summary>
	/// Configures links policy
	/// </summary>
	public static class LinkExtension
	{
		private static readonly LinkBuilder linkBuilder = new LinkBuilder();

		/// <summary>
		/// Adds links policy services required for policy configuration.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
		/// <param name="configure">An <see cref="Action{LinkBuilder}"/> to configure the provided <see cref="LinkBuilder"/>.</param>
		/// <returns>An <see cref="IServiceCollection"/> instance with required services registered.</returns>
		public static IServiceCollection AddLink(
			this IServiceCollection services,
			Action<LinkBuilder> configure = null)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddTransient<HateoasUriProvider<InMemoryPolicyRepository.SelfPolicy>, HateoasSelfUriProvider>();
			services.AddTransient<HateoasUriProvider<InMemoryPolicyRepository.RoutePolicy>, HateoasRouteUriProvider>();
			services.AddTransient<HateoasUriProvider<InMemoryPolicyRepository.CustomPolicy>, HateoasCustomUriProvider>();
			services.AddTransient<HateoasUriProvider<InMemoryPolicyRepository.ExternalPolicy>, HateoasExternalUriProvider>();
			//added by j3antov4r a route provider for collection level policies
			services.AddTransient<HateoasUriProvider<InMemoryPolicyRepository.CollectionLevelPolicy>, HateoasCollectionLevelUriProvider>();

			services.AddTransient<IHateoasResultProvider, HateoasResultProvider>();

			services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add<LocationUriResultFilterAttribute>();
				options.Filters.Add<HateoasResultFilterAttribute>();
			});

			configure?.Invoke(linkBuilder);

			return services;
		}

		/// <summary>
		/// Adds links policy services required for policy configuration.
		/// </summary>
		/// <param name="mvcBuilder">The <see cref="IMvcBuilder" /> services to add services to.</param>
		/// <param name="configure">An <see cref="Action{LinkBuilder}"/> to configure the provided <see cref="LinkBuilder"/>.</param>
		/// <returns>An <see cref="IMvcBuilder"/> instance with required services registered.</returns>
		public static IMvcBuilder AddLink(this IMvcBuilder mvcBuilder, Action<LinkBuilder> configure = null)
		{
			mvcBuilder.Services.AddLink(configure);
			return mvcBuilder;
		}
	}
}
