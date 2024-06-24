using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Sciensoft.Hateoas.Providers;
using Sciensoft.Hateoas.Repositories;

namespace Sciensoft.Hateoas
{
    internal class HateoasCollectionLevelUriProvider : HateoasUriProvider<InMemoryPolicyRepository.CollectionLevelPolicy>
    {
        private readonly IActionDescriptorCollectionProvider _actionsProvider;
        public HateoasCollectionLevelUriProvider(IHttpContextAccessor contextAccessor, LinkGenerator linkGenerator, IActionDescriptorCollectionProvider actionsProvider) : base(contextAccessor, linkGenerator)
        {
            _actionsProvider = actionsProvider ?? throw new ArgumentNullException(nameof(actionsProvider));
        }

        public override (string Method, string Uri) GenerateEndpoint(InMemoryPolicyRepository.CollectionLevelPolicy policy, object result)
        {
            
            string targetRouteName = policy.RouteName;
			var routeData = HttpContext.GetRouteData();

			var controllerName = routeData.Values.FirstOrDefault(rv => rv.Key.Equals("controller")).Value?.ToString();
			if (string.IsNullOrWhiteSpace(controllerName))
			{
				return default;
			}

			var controllerDescriptor = _actionsProvider.ActionDescriptors.Items
				.Where(r => r.RouteValues.Any(rv => rv.Value.Equals(controllerName)));

			var routeInfo = controllerDescriptor
				.FirstOrDefault(r =>
					r.AttributeRouteInfo != null
					&& r.AttributeRouteInfo.Name != null
					&& (r.AttributeRouteInfo.Name.Equals(targetRouteName)));

			if (routeInfo == null)
			{
				targetRouteName = null;
				routeInfo = controllerDescriptor
					.FirstOrDefault(c => c.EndpointMetadata.Any(e =>
						(e is HttpMethodAttribute)
						&& ((HttpMethodAttribute)e).Name != null
						&& ((HttpMethodAttribute)e).Name.Equals(policy.RouteName)));
			}

			var localRouteValues = routeInfo.RouteValues.ToDictionary(r => r.Key, r => r.Value);

			var expressionMember = (((policy.Expression as LambdaExpression)?.Body as UnaryExpression)?.Operand as MemberExpression)?.Member;
			if(expressionMember != null && !string.IsNullOrEmpty(expressionMember.Name))
                localRouteValues.TryAdd(expressionMember.Name, result.ToString());

			var httpMethodMetadata = routeInfo.EndpointMetadata.FirstOrDefault(x => x is HttpMethodMetadata) as HttpMethodMetadata;
			var httpMethod = httpMethodMetadata.HttpMethods.FirstOrDefault();

			string virtualPath = LinkGenerator.GetPathByRouteValues(HttpContext, targetRouteName, localRouteValues);
			string finalPath = GetFormatedPath($"{virtualPath}");

			return (httpMethod, $"{Host}/{finalPath}");
        }
    }
}
