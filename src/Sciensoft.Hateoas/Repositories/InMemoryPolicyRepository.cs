using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Sciensoft.Hateoas.Repositories
{
	/// <summary>
	/// Repository for policies of type <see cref="Policy"/>.
	/// </summary>
	public sealed class InMemoryPolicyRepository
	{
		/// <summary>
		/// List of registered policies.
		/// </summary>
		public static ConcurrentBag<Policy> InMemoryPolicies { get; } = new ConcurrentBag<Policy>();

		/// <summary>
		/// Abstract policy for link generation
		/// </summary>
		public abstract class Policy
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="Policy"/> with the configuration and arguments provided.
			/// </summary>
			/// <param name="type">A model type.</param>
			/// <param name="expression">An expression for link generation.</param>
			/// <param name="name">An optional name of the link policy.</param>
			/// <param name="memberName">The policy name of the caller merber name.</param>
			protected Policy(Type type, Expression expression, string name = null, [CallerMemberName] string memberName = null)
			{
				if (string.IsNullOrWhiteSpace(name))
				{
					name = memberName;
				}

				this.Type = type ?? throw new ArgumentNullException(nameof(type));
				this.Expression = expression ?? throw new ArgumentNullException(nameof(expression));

				this.Name = name;
			}

			/// <summary>
			/// The model type for the link policy.
			/// </summary>
			public Type Type { get; }

			/// <summary>
			/// The expression for link generation.
			/// </summary>
			public Expression Expression { get; }

			/// <summary>
			/// The name of the link policy.
			/// </summary>
			public string Name { get; }

			/// <summary>
			/// An message for the link policy.
			/// </summary>
			public string Message { get; set; }

			/// <summary>
			/// The HTTP method for the link policy.
			/// </summary>
			public string Method { get; set; }
		}

		internal class SelfPolicy : Policy
		{
			public SelfPolicy(Type type, Expression expression, string name = null, [CallerMemberName] string memberName = null)
				: base(type, expression, name, memberName)
			{ }

			public string Template { get; set; } = "/";
		}

		internal class RoutePolicy : Policy
		{
			public RoutePolicy(Type type, Expression expression, string routeName, [CallerMemberName] string memberName = null)
				: base(type, expression, routeName, memberName)
			{
				if (string.IsNullOrWhiteSpace(routeName))
				{
					throw new ArgumentNullException(nameof(routeName));
				}

				RouteName = routeName;
			}

			public string RouteName { get; }
		}

		internal class CustomPolicy : Policy
		{
			public CustomPolicy(Type type, Expression expression, string name = null, [CallerMemberName] string memberName = null)
				: base(type, expression, name, memberName)
			{ }
		}

		internal class ExternalPolicy : Policy
		{
			public string[] Hosts { get; set; }

			public ExternalPolicy(Type type, Expression expression, string host, string name = null, [CallerMemberName] string memberName = null)
				: base(type, expression, name, memberName)
			{
				if (string.IsNullOrWhiteSpace(host))
				{
					throw new ArgumentNullException(nameof(host));
				}

				Hosts = new[] { host };
			}
		}

		//added by me
		internal class CollectionLevelPolicy : Policy
		{
			public CollectionLevelPolicy(Type type, Expression expression, string routeName, [CallerMemberName] string memberName = null)
				: base(type, expression, routeName, memberName)
			{ 
				if (string.IsNullOrWhiteSpace(routeName))
				{
					throw new ArgumentNullException(nameof(routeName));
				}

				RouteName = routeName;
			}

			public string RouteName { get; }
		}
	}
}
