using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Dynamics
{
	public static class Resolver
	{
		#region Methods / Public

		public static MethodInfo	Method<T> (Expression<Action<T>> lambda)
		{
			return Resolver.ResolveMethod<Action<T>> (lambda);
		}

		public static MethodInfo	Method<T, U> (Expression<Action<T, U>> lambda)
		{
			return Resolver.ResolveMethod<Action<T, U>> (lambda);
		}

		public static MethodInfo	Method<T, U, V> (Expression<Action<T, U, V>> lambda)
		{
			return Resolver.ResolveMethod<Action<T, U, V>> (lambda);
		}

		public static MethodInfo	Method<T, U, V, W> (Expression<Action<T, U, V, W>> lambda)
		{
			return Resolver.ResolveMethod<Action<T, U, V, W>> (lambda);
		}

		public static MethodInfo	Method<T> (Expression<Func<T>> lambda)
		{
			return Resolver.ResolveMethod<Func<T>> (lambda);
		}

		public static MethodInfo	Method<T, U> (Expression<Func<T, U>> lambda)
		{
			return Resolver.ResolveMethod<Func<T, U>> (lambda);
		}

		public static MethodInfo	Method<T, U, V> (Expression<Func<T, U, V>> lambda)
		{
			return Resolver.ResolveMethod<Func<T, U, V>> (lambda);
		}

		public static MethodInfo	Method<T, U, V, W> (Expression<Func<T, U, V, W>> lambda)
		{
			return Resolver.ResolveMethod<Func<T, U, V, W>> (lambda);
		}

		public static PropertyInfo	Property<T> (Expression<Func<T>> lambda)
		{
			return Resolver.ResolveProperty<Func<T>> (lambda);
		}

		public static PropertyInfo	Property<T, U> (Expression<Func<T, U>> lambda)
		{
			return Resolver.ResolveProperty<Func<T, U>> (lambda);
		}

		public static PropertyInfo	Property<T, U, V> (Expression<Func<T, U, V>> lambda)
		{
			return Resolver.ResolveProperty<Func<T, U, V>> (lambda);
		}

		public static PropertyInfo	Property<T, U, V, W> (Expression<Func<T, U, V, W>> lambda)
		{
			return Resolver.ResolveProperty<Func<T, U, V, W>> (lambda);
		}

		#endregion
		
		#region Methods / Private

		private static MethodInfo	ResolveMethod<T> (Expression<T> lambda)
	    {
	    	MethodCallExpression	expression;

	        expression = lambda.Body as MethodCallExpression;

	        if (expression == null)
	        	throw new ArgumentException ("can't get method information from expression", "lambda");

	        return expression.Method;
	    }

		private static PropertyInfo	ResolveProperty<T> (Expression<T> lambda)
		{
	    	MemberExpression	expression;

	        expression = lambda.Body as MemberExpression;

	        if (expression == null || expression.Member.MemberType != MemberTypes.Property)
	        	throw new ArgumentException ("can't get property information from expression", "lambda");

	        return (PropertyInfo)expression.Member;
		}

		#endregion
	}
}
