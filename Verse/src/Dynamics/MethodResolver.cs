using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Verse.Dynamics
{
	static class MethodResolver
	{
		#region Methods
		
		public static MethodInfo	Resolve<T> (Expression<T> expression)
	    {
	    	MethodCallExpression	call;
	    	MethodInfo				method;

	        call = expression.Body as MethodCallExpression;

	        if (call == null)
	        	throw new ArgumentException ("can't get method information from lambda expression", "expression");

	        method = call.Method;

	        if (method.IsGenericMethod)
	        	method = method.GetGenericMethodDefinition ();

	        return method;
	    }

		#endregion
	}
}
