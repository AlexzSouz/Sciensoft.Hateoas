using Microsoft.AspNetCore.Mvc;
using Sciensoft.Hateoas.Extensions;
using Sciensoft.Hateoas.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Sciensoft.Hateoas.Providers
{
	internal class HateoasResultProvider : IHateoasResultProvider
	{
		private readonly IServiceProvider _serviceProvider;

		public HateoasResultProvider(IServiceProvider serviceProvider)
			=> _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

		public bool HasAnyPolicy(IActionResult actionResult, out ObjectResult objectResult)
		{
			if (actionResult is ObjectResult result)
			{
				objectResult = actionResult as ObjectResult;
				string resultType = objectResult.Value.GetType().FullName;

				if (result.Value is IEnumerable<object> collection)
				{
					resultType = collection
						.Select(v => v.GetType().FullName)
						.Distinct()
						.Single();
				}

				return InMemoryPolicyRepository.InMemoryPolicies
					.Any(p => p.Type.FullName.Equals(resultType));
			}

			objectResult = default;
			return false;
		}

		public async Task<IActionResult> GetContentResultAsync(ObjectResult result)
		{
			var policies = GetFilteredPolicies(result);
			if (!policies.Any())
			{
				return null;
			}

			var content = default(JsonResult);

			async Task<IList<object>> GetLinksAsync(object item)
			{
				var links = new List<object>();

				foreach (var policy in policies.Where(p => p != null && !(p is InMemoryPolicyRepository.CollectionLevelPolicy)))
				{
					var lambdaResult = GetLambdaResult(policy.Expression, item);
					var link = await GetPolicyLinkAsync(policy, lambdaResult).ConfigureAwait(false);
					links.Add(link);
				}

				return links;
			}

			async Task<object> GetFinalJsonPayloadAsync(object item)
			{
				var links = await GetLinksAsync(item).ConfigureAwait(false);

				return await Task.FromResult(item.ToFinalPayload(links)).ConfigureAwait(false);
			}

			if (result.Value is IEnumerable<object> collection)
			{
				var links = await Task.WhenAll(collection.Select(item => GetFinalJsonPayloadAsync(item))).ConfigureAwait(false);
				var json = new List<object>(links);
				
				content = new JsonResult(json);

				//add collection level links if any
				var collectionLevelPolicies = policies.Where(p => p is InMemoryPolicyRepository.CollectionLevelPolicy);
				if (collectionLevelPolicies.Any())
				{
					var collectionLinks = new List<object>();
					foreach (var policy in collectionLevelPolicies)
					{
						var lambdaResult = GetLambdaResult(policy.Expression, collection);
						var link = await GetPolicyLinkAsync(policy, lambdaResult).ConfigureAwait(false);
						collectionLinks.Add(link);
					}
					//modify the content to include the collection level links at the root
					content = new JsonResult(content.Value.ToFinalPayload(collectionLinks));
				}
			}
			else
			{
				var links = await GetFinalJsonPayloadAsync(result.Value).ConfigureAwait(false);
				content = new JsonResult(links);
			}

			return await Task.FromResult(content).ConfigureAwait(false);
		}

		private async Task<object> GetPolicyLinkAsync(InMemoryPolicyRepository.Policy policy, object model)
		{
			var uriProviderType = typeof(HateoasUriProvider<>).MakeGenericType(policy.GetType());
			var uriProvider = _serviceProvider.GetService(uriProviderType);

			var endpoint = (dynamic)uriProvider.GetType()
				.InvokeMember(
					nameof(HateoasUriProvider<InMemoryPolicyRepository.Policy>.GenerateEndpoint),
					BindingFlags.InvokeMethod,
					null,
					uriProvider,
					new[] { policy, model });

			var link = new
			{
				Method = endpoint.Item1,
				Uri = endpoint.Item2,
				Relation = policy.Name,
				policy.Message
			};

			return await Task.FromResult(link).ConfigureAwait(false);
		}

		private object GetLambdaResult(Expression expression, object sourcePayload)
		{
			var lambdaExpression = (expression as LambdaExpression);

			if (lambdaExpression == null)
				return null;

			var body = lambdaExpression.Body;
			var parameter = lambdaExpression.Parameters[0];

			//verificar si sourcePayload es IEnumerable, Array o ICollection
			//si eas asi entoces cambiar sourcePayload por sourcePayload.First()
			if(sourcePayload is IEnumerable<object> collection)
			{
				sourcePayload = collection.First();
			}

			return Expression.Lambda(body, parameter).Compile().DynamicInvoke(sourcePayload);
		}

		
		private IEnumerable<InMemoryPolicyRepository.Policy> GetFilteredPolicies(ObjectResult result)
		{
			string resultType = result.Value.GetType().FullName;

			if (result.Value is IEnumerable<object> collection)
			{
				resultType = collection
					.Select(v => v.GetType().FullName)
					.Distinct()
					.Single();
			}

			return InMemoryPolicyRepository.InMemoryPolicies
				.Where(p => p.Type.FullName.Equals(resultType))
				.AsEnumerable();
		}
	}
}
