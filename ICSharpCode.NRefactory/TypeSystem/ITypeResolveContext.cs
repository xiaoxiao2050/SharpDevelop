﻿// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ICSharpCode.NRefactory.TypeSystem
{
	/// <summary>
	/// Context representing the set of assemblies in which a type is being searched.
	/// </summary>
	[ContractClass(typeof(ITypeResolveContextContract))]
	public interface ITypeResolveContext
	{
		/// <summary>
		/// Retrieves a class.
		/// </summary>
		/// <param name="fullTypeName">Full name of the class</param>
		/// <param name="typeParameterCount">Number of type parameters</param>
		/// <param name="nameComparer">Language-specific rules for how class names are compared</param>
		/// <returns>The type definition for the class; or null if no such class exists.</returns>
		/// <remarks>This method never returns inner classes; it can be used only with top-level classes.</remarks>
		ITypeDefinition GetClass(string fullTypeName, int typeParameterCount, StringComparer nameComparer);
		
		/// <summary>
		/// Retrieves all top-level classes.
		/// </summary>
		/// <remarks>
		/// If this method is called within <c>using (pc.Synchronize())</c>, then the returned enumerable is valid
		/// only until the end of the synchronize block.
		/// </remarks>
		IEnumerable<ITypeDefinition> GetClasses();
		
		/// <summary>
		/// Retrieves all classes in the specified namespace.
		/// </summary>
		/// <param name="nameSpace">Namespace in which classes are being retrieved. Use <c>string.Empty</c> for the root namespace.</param>
		/// <param name="nameComparer">Language-specific rules for how namespace names are compared</param>
		/// <returns>List of classes within that namespace.</returns>
		/// <remarks>
		/// If this method is called within <c>using (pc.Synchronize())</c>, then the returned enumerable is valid
		/// only until the end of the synchronize block.
		/// </remarks>
		IEnumerable<ITypeDefinition> GetClasses(string nameSpace, StringComparer nameComparer);
		
		/// <summary>
		/// Retrieves all namespaces.
		/// </summary>
		/// <remarks>
		/// If this method is called within <c>using (pc.Synchronize())</c>, then the returned enumerable is valid
		/// only until the end of the synchronize block.
		/// </remarks>
		IEnumerable<string> GetNamespaces();
		
		/// <summary>
		/// Returns a <see cref="ISynchronizedTypeResolveContext"/> that
		/// represents the same context as this instance, but cannot be modified
		/// by other threads.
		/// The ISynchronizedTypeResolveContext must be disposed from the same thread
		/// that called this method when it is no longer used.
		/// </summary>
		/// <remarks>
		/// A simple implementation might enter a ReaderWriterLock when the synchronized context
		/// is created, and releases the lock when Dispose() is called.
		/// However, implementations based on immutable data structures are also possible.
		/// </remarks>
		ISynchronizedTypeResolveContext Synchronize();
	}
	
	[ContractClassFor(typeof(ITypeResolveContext))]
	abstract class ITypeResolveContextContract : ITypeResolveContext
	{
		ITypeDefinition ITypeResolveContext.GetClass(string fullTypeName, int typeParameterCount, StringComparer nameComparer)
		{
			Contract.Requires(fullTypeName != null);
			Contract.Requires(typeParameterCount >= 0);
			Contract.Requires(nameComparer != null);
			throw new NotImplementedException();
		}
		
		ISynchronizedTypeResolveContext ITypeResolveContext.Synchronize()
		{
			Contract.Ensures(Contract.Result<ISynchronizedTypeResolveContext>() != null);
			throw new NotImplementedException();
		}
		
		IEnumerable<ITypeDefinition> ITypeResolveContext.GetClasses()
		{
			Contract.Ensures(Contract.Result<IEnumerable<ITypeDefinition>>() != null);
			throw new NotImplementedException();
		}
		
		IEnumerable<ITypeDefinition> ITypeResolveContext.GetClasses(string nameSpace, StringComparer nameComparer)
		{
			Contract.Requires(nameSpace != null);
			Contract.Requires(nameComparer != null);
			Contract.Ensures(Contract.Result<IEnumerable<ITypeDefinition>>() != null);
			throw new NotImplementedException();
		}
		
		IEnumerable<string> ITypeResolveContext.GetNamespaces()
		{
			Contract.Ensures(Contract.Result<IEnumerable<ITypeDefinition>>() != null);
			throw new NotImplementedException();
		}
	}
}
