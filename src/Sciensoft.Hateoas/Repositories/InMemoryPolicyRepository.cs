﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Sciensoft.Hateoas.Repositories
{
	internal sealed class InMemoryPolicyRepository
	{
		public static IList<Policy> InMemoryPolicies { get; } = new List<Policy>();

		public abstract class Policy
		{
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

			public Type Type { get; }

			public Expression Expression { get; }

			public string Name { get; }

			public string Message { get; set; }

			public string Method { get; set; }
		}

		public class SelfPolicy : Policy
		{
			public SelfPolicy(Type type, Expression expression, string name = null, [CallerMemberName] string memberName = null)
				: base(type, expression, name, memberName)
			{ }

			public string Template { get; set; } = "/";
		}

		public class CustomPolicy : Policy
		{
			public CustomPolicy(Type type, Expression expression, string name = null, [CallerMemberName] string memberName = null)
				: base(type, expression, name, memberName)
			{ }
		}

		public class RoutePolicy : Policy
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
	}
}