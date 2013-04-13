using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Dynamics
{
	public static class Resolver
	{
		#region Methods / Public

		public static MethodInfo	Method<T> (Expression<T> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.ResolveMethod<T> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<T> (Expression<Action<T>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.ResolveMethod<Action<T>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<T, U> (Expression<Action<T, U>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.ResolveMethod<Action<T, U>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<T, U, V> (Expression<Action<T, U, V>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.ResolveMethod<Action<T, U, V>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<T, U, V, W> (Expression<Action<T, U, V, W>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.ResolveMethod<Action<T, U, V, W>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<T> (Expression<Func<T>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.ResolveMethod<Func<T>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<T, U> (Expression<Func<T, U>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.ResolveMethod<Func<T, U>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<T, U, V> (Expression<Func<T, U, V>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.ResolveMethod<Func<T, U, V>> (lambda, targetParameters, methodParameters);
		}

		public static MethodInfo	Method<T, U, V, W> (Expression<Func<T, U, V, W>> lambda, Type[] targetParameters = null, Type[] methodParameters = null)
		{
			return Resolver.ResolveMethod<Func<T, U, V, W>> (lambda, targetParameters, methodParameters);
		}

		public static PropertyInfo	Property<T> (Expression<Func<T>> lambda, Type[] targetParameters = null)
		{
			return Resolver.ResolveProperty<Func<T>> (lambda, targetParameters);
		}

		public static PropertyInfo	Property<T, U> (Expression<Func<T, U>> lambda, Type[] targetParameters = null)
		{
			return Resolver.ResolveProperty<Func<T, U>> (lambda, targetParameters);
		}

		public static PropertyInfo	Property<T, U, V> (Expression<Func<T, U, V>> lambda, Type[] targetParameters = null)
		{
			return Resolver.ResolveProperty<Func<T, U, V>> (lambda, targetParameters);
		}

		public static PropertyInfo	Property<T, U, V, W> (Expression<Func<T, U, V, W>> lambda, Type[] targetParameters = null)
		{
			return Resolver.ResolveProperty<Func<T, U, V, W>> (lambda, targetParameters);
		}

		#endregion
		
		#region Methods / Private

		private static MethodInfo	ResolveMethod<T> (Expression<T> lambda, Type[] targetParameters, Type[] methodParameters)
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

		private static PropertyInfo	ResolveProperty<T> (Expression<T> lambda, Type[] targetParameters)
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
}
