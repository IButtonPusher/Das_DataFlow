using System;
using System.Collections;
using System.Collections.Generic;

namespace Das.DataFlow
{
	internal interface ITypeFinder
	{
		Boolean ImplementsGenericInterface<TInterface>(Object obj, Type[] genericTypes);

		IEnumerable<Type> GetGenericImplementations<TInterface>(Object obj);

		IEnumerable<TBaseImplementation> GetImplementations<TBaseImplementation>(
			IEnumerable<Type> forTypes, IEnumerable<IEnumerable> fromObjects);

		TBaseImplementation GetImplementation<TBaseImplementation>(Type genericType,
			IEnumerable<IEnumerable> fromObjects);

		TGenericResult GetGenericImplementation<TBaseImplementation, TGenericResult>
			(IEnumerable fromObjects) where TGenericResult : TBaseImplementation;

		TResult GetImplementationWithMatchingConstraints<TInput, TResult>(TInput forOjbect,
			IEnumerable fromObjects);
	}
}