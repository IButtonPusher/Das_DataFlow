using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Das.DataFlow
{
    public class TypeFinder : ITypeFinder
	{
		public Boolean ImplementsGenericInterface<TInterface>(Object obj, 
			params Type[] genericTypes)
		{
			var interfaceType = typeof(TInterface);
            var interfaces = obj.GetType().GetInterfaces();

			foreach (var i in interfaces)
			{
				if (!i.IsGenericType)
					continue;

                if (i.IsAssignableFrom(interfaceType))
                { }

				if (interfaceType.IsAssignableFrom(i))
				{
					var interfaceGenerics = i.GetGenericArguments();

					if (IsGenericsMatch(interfaceGenerics, genericTypes))
						return true;

					//if (interfaceGenerics.SequenceEqual(genericTypes))
					//	return true;

//					if (interfaceGenerics.Length != genericTypes.Length)
//						continue;
//
//					for (var j = 0; j < interfaceGenerics.Length; j++)
//					{
//						if (!genericTypes[j].IsAssignableFrom(interfaceGenerics[j]))
//							break;
//						//if (genericType.IsAssignableFrom(.Last()))
//						//	return true;
//					}
//					return true;
				}
			}

			return false;
		}

		private Boolean IsGenericsMatch(Type[] left, Type[] right)
		{
			if (left.Length != right.Length)
				return false;

			for (var j = 0; j < right.Length; j++)
			{
				if (!right[j].IsAssignableFrom(left[j]))
					return false;
			}

			return true;
		}

		public IEnumerable<Type> GetGenericImplementations<TInterface>(Object obj)
		{
			var type = typeof(TInterface);

			foreach (var i in obj.GetType().GetInterfaces())
			{
				if (!i.IsGenericType)
					continue;
				if (!type.IsAssignableFrom(i))
					continue;
				if (i.GetInterfaces().All(inter => inter != type))
					continue;

				String typeName = i.Name;
				var endName = typeName.IndexOf("`", StringComparison.Ordinal);
				if (endName > 0)
					typeName = typeName.Substring(0, endName);

				if (typeName.Equals(type.Name))
					yield return i;
			}
		}

		public IEnumerable<TBaseImplementation> GetImplementations<TBaseImplementation>(
			IEnumerable<Type> forTypes, IEnumerable<IEnumerable> fromObjects)
        {
            var objArray = fromObjects.ToArray();

			foreach (var type in forTypes)
			{
				TBaseImplementation found = default;

				foreach (var list in objArray)
				{
					foreach (var obj in list)
					{
						if (ImplementsGenericInterface<TBaseImplementation>(obj, 
							type.GetGenericArguments().First()))
							found = (TBaseImplementation)obj;
					}
				}

				if (found != null)
					yield return found;
			}
		}

		public TBaseImplementation GetImplementation<TBaseImplementation>(Type genericType,
			IEnumerable<IEnumerable> fromObjects)
		{
			TBaseImplementation found = default;

			foreach (var list in fromObjects)
			{
				foreach (var obj in list)
				{
					if (ImplementsGenericInterface<TBaseImplementation>(obj, genericType))
						found = (TBaseImplementation)obj;
				}
			}

			return found;
		}

		public TGenericResult GetGenericImplementation<TBaseImplementation, TGenericResult>(
			IEnumerable fromObjects) 
			where TGenericResult : TBaseImplementation
		{
			var gTypes = typeof(TGenericResult).GetGenericArguments();

			foreach (var obj in fromObjects)
			{
				if (ImplementsGenericInterface<TBaseImplementation>(obj, gTypes))
					return (TGenericResult)obj;
			}

			return default;
		}

		public TResult GetImplementationWithMatchingConstraints<TInput, TResult>(
			TInput forOjbect, IEnumerable fromObjects)
		{
			//var gTypes = typeof(TInput).GetGenericArguments();
			var gTypes = forOjbect.GetType().GetGenericArguments();

			foreach (var obj in fromObjects)
			{
				if (ImplementsGenericInterface<TResult>(obj, gTypes))
					return (TResult)obj;
			}

			return default;
		}
	}
}
