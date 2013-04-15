using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Dynamics
{
	public static class Resolver
	{
		#region Methods

		public static MethodInfo	GetMethod<T> (Expression<T> lambda, Type[] targetParameters, Type[] methodParameters)
	    {
	    	MethodCallExpression	expression;
	    	MethodInfo				method;
	    	Type					type;

	        expression = lambda.Body as MethodCallExpression;

	        if (expression == null)
	        	throw new ArgumentException ("can't get method information from expression", "lambda");

			method = expression.Method;

			// Change method generic parameters if requested
			if (methodParameters != null && method.IsGenericMethod)
			{
				method = method.GetGenericMethodDefinition ();

				if (methodParameters.Length > 0)
					method = method.MakeGenericMethod (methodParameters);
			}

			// Change target generic parameters if requested
			if (targetParameters != null && method.DeclaringType.IsGenericType)
			{
				type = method.DeclaringType.GetGenericTypeDefinition ();

				if (targetParameters.Length > 0)
					type = type.MakeGenericType (targetParameters);

				method = Array.Find (type.GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static), (m) => m.MetadataToken == method.MetadataToken);
			}

	        return method;
	    }

		public static PropertyInfo	GetProperty<T> (Expression<T> lambda, Type[] targetParameters)
		{
	    	MemberExpression	expression;
	    	PropertyInfo		property;
	    	Type				type;

	        expression = lambda.Body as MemberExpression;

	        if (expression == null || expression.Member.MemberType != MemberTypes.Property)
	        	throw new ArgumentException ("can't get property information from expression", "lambda");

	        property = (PropertyInfo)expression.Member;

			// Change target generic parameters if requested
			if (targetParameters != null && property.DeclaringType.IsGenericType)
			{
				type = property.DeclaringType.GetGenericTypeDefinition ();

				if (targetParameters.Length > 0)
					type = type.MakeGenericType (targetParameters);

				property = Array.Find (type.GetProperties (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static), (p) => p.MetadataToken == property.MetadataToken);
			}

			return property;
		}

		#endregion
	}

	public static class Resolver<T>
	{
		#region Methods

		public static MethodInfo	Method (Expression<Action<T>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.GetMethod<Action<T>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<U0> (Expression<Action<T, U0>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.GetMethod<Action<T, U0>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<U0, U1> (Expression<Action<T, U0, U1>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.GetMethod<Action<T, U0, U1>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<U0, U1, U2> (Expression<Action<T, U0, U1, U2>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.GetMethod<Action<T, U0, U1, U2>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<U0, U1, U2, U3> (Expression<Action<T, U0, U1, U2, U3>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.GetMethod<Action<T, U0, U1, U2, U3>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<U0> (Expression<Func<T, U0>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.GetMethod<Func<T, U0>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<U0, U1> (Expression<Func<T, U0, U1>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.GetMethod<Func<T, U0, U1>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<U0, U1, U2> (Expression<Func<T, U0, U1, U2>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.GetMethod<Func<T, U0, U1, U2>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<U0, U1, U2, U3> (Expression<Func<T, U0, U1, U2, U3>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.GetMethod<Func<T, U0, U1, U2, U3>> (lambda, targetParameters, methodParameters);
		}

		public static PropertyInfo	Property<U0> (Expression<Func<T, U0>> lambda, Type[] targetParameters = null)
		{
			return Resolver.GetProperty<Func<T, U0>> (lambda, targetParameters);
		}

		public static PropertyInfo	Property<U0, U1> (Expression<Func<T, U0, U1>> lambda, Type[] targetParameters = null)
		{
			return Resolver.GetProperty<Func<T, U0, U1>> (lambda, targetParameters);
		}

		public static PropertyInfo	Property<U0, U1, U2> (Expression<Func<T, U0, U1, U2>> lambda, Type[] targetParameters = null)
		{
			return Resolver.GetProperty<Func<T, U0, U1, U2>> (lambda, targetParameters);
		}

		public static PropertyInfo	Property<U0, U1, U2, U3> (Expression<Func<T, U0, U1, U2, U3>> lambda, Type[] targetParameters = null)
		{
			return Resolver.GetProperty<Func<T, U0, U1, U2, U3>> (lambda, targetParameters);
		}

		#endregion
	}
}
