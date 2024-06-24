using Microsoft.AspNetCore.Http;
using Sciensoft.Hateoas.Constants;
using Sciensoft.Hateoas.Exceptions;
using Sciensoft.Hateoas.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sciensoft.Hateoas
{
	/// <summary>
	/// An <see cref="PolicyBuilder{T}"/> to add the policies to.
	/// </summary>
	/// <typeparam name="T">An view-model object type to configure the policies to.</typeparam>
	public sealed class PolicyBuilder<T>
		where T : class
	{
		/// <summary>
		/// Adds self URI to the resource representation.
		/// </summary>
		/// <param name="expression">An expression for link generation, e.g. <code>l => l.Id</code>.</param>
		/// <param name="message">A descriptive message for the link.</param>
		/// <returns><see cref="PolicyBuilder{T}"/> with configured policies.</returns>
		public PolicyBuilder<T> AddSelf(Expression<Func<T, object>> expression, string message = null)
		{
			InMemoryPolicyRepository.InMemoryPolicies.Add(
				new InMemoryPolicyRepository.SelfPolicy(typeof(T), expression, PolicyConstants.Self)
				{
					Method = HttpMethods.Get,
					Message = message
				});

			return this;
		}

		/// <summary>
		/// Adds route based URI to the resource representation.
		/// </summary>
		/// <param name="expression">An expression for link generation, e.g. <code>l => l.Id</code>.</param>
		/// <param name="routeName">The attribute or conventional route name.</param>
		/// <param name="method">Resource method as per HTTP methods, e.g. GET/HEAD/POST, visit <inheritdoc path="https://tools.ietf.org/html/rfc7231#section-4"/> for more information.</param>
		/// <param name="message">A descriptive message for the link.</param>
		/// <returns><see cref="PolicyBuilder{T}"/> with configured policies.</returns>
		public PolicyBuilder<T> AddRoute(Expression<Func<T, object>> expression, string routeName, string method = null, string message = null)
		{
			if (string.IsNullOrWhiteSpace(routeName))
			{
				throw new InvalidPolicyConfigurationException($"Routed policy requires '{nameof(routeName)}' argument.");
			}

			InMemoryPolicyRepository.InMemoryPolicies.Add(
				new InMemoryPolicyRepository.RoutePolicy(typeof(T), expression, routeName)
				{
					Method = method ?? HttpMethods.Get,
					Message = message
				});

			return this;
		}

		/// <summary>
		/// Adds custom defined URI to the resource representation, e.g., would be pointing to an external URI.
		/// </summary>
		/// <remarks>
		/// It's possible to use tokens E.g. [controller], [action], [your-own] to match and replace by the route values.
		/// </remarks>
		/// <param name="expression">An expression for link generation, e.g. <code>l => $"/api/resource-path/{l.id}"</code>.</param>
		/// <param name="linkKey">Link identifier.</param>
		/// <param name="method">Resource method as per HTTP methods, e.g. GET/HEAD/POST, visit <inheritdoc path="https://tools.ietf.org/html/rfc7231#section-4"/> for more information.</param>
		/// <param name="message">A descriptive message for the link.</param>
		/// <returns><see cref="PolicyBuilder{T}"/> with configured policies.</returns>
		public PolicyBuilder<T> AddCustomPath(Expression<Func<T, object>> expression, string linkKey, string method = null, string message = null)
		{
			if (string.IsNullOrWhiteSpace(linkKey))
			{
				throw new InvalidPolicyConfigurationException($"Custom Policy requires '{nameof(linkKey)}' argument.");
			}

			InMemoryPolicyRepository.InMemoryPolicies.Add(
				new InMemoryPolicyRepository.CustomPolicy(typeof(T), expression, linkKey)
				{
					Method = method ?? HttpMethods.Get,
					Message = message
				});

			return this;
		}

		/// <summary>
		/// Adds custom defined URI to the resource representation, e.g., would be pointing to an external URI.
		/// </summary>
		/// <remarks>
		/// It's possible to use tokens E.g. [controller], [action], [your-own] to match and replace by the route values.
		/// </remarks>
		/// <param name="expression">An expression for link generation, e.g. <code>l => $"https://my-external-uri.com/api/resource/{l.Id}"</code>.</param>
		/// <param name="host">List of external hosts for link generation.</param>
		/// <param name="linkKey">Link identifier.</param>
		/// <param name="method">Resource method as per HTTP methods, e.g. GET/HEAD/POST, visit <inheritdoc path="https://tools.ietf.org/html/rfc7231#section-4"/> for more information.</param>
		/// <param name="message">A descriptive message for the link.</param>
		/// <returns><see cref="PolicyBuilder{T}"/> with configured policies.</returns>
		public PolicyBuilder<T> AddExternalUri(Expression<Func<T, object>> expression, string host, string linkKey, string method = null, string message = null)
		{
			if (string.IsNullOrWhiteSpace(host))
			{
				throw new InvalidPolicyConfigurationException($"External Policy requires '{nameof(host)}' argument.");
			}

			if (string.IsNullOrWhiteSpace(linkKey))
			{
				throw new InvalidPolicyConfigurationException($"External Policy requires '{nameof(linkKey)}' argument.");
			}

			InMemoryPolicyRepository.InMemoryPolicies.Add(
				new InMemoryPolicyRepository.ExternalPolicy(typeof(T), expression, host, linkKey)
				{
					Method = method ?? HttpMethods.Get,
					Message = message
				});

			return this;
		}

		/// <summary>
		/// Adds route based URI to a resource collection. 
		/// </summary>
		/// <author>
		/// Jean Tovar GitHub: j3antov4r
		///</author>  
		/// <param name="expression">An expression for link generation, e.g. <code>l => l.Id</code>.</param>
		/// <param name="routeName">The attribute or conventional route name.</param>
		/// <param name="method">Resource method as per HTTP methods, e.g. GET/HEAD/POST, visit <inheritdoc path="https://tools.ietf.org/html/rfc7231#section-4"/> for more information.</param>
		/// <param name="message">A descriptive message for the link.</param>
		/// <returns><see cref="PolicyBuilder{T}"/> with configured policies.</returns>
		public PolicyBuilder<T> AddCollectionLevel(Expression<Func<T, object>> expression, string routeName, string method = null, string message = null)
		{
			if (string.IsNullOrWhiteSpace(routeName))
			{
				throw new InvalidPolicyConfigurationException($"Routed policy requires '{nameof(routeName)}' argument.");
			}

			InMemoryPolicyRepository.InMemoryPolicies.Add(
				new InMemoryPolicyRepository.CollectionLevelPolicy(typeof(T), expression, routeName)
				{
					Method = method ?? HttpMethods.Get,
					Message = message
				});

			return this;
		}
	}
}