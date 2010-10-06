﻿// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;

namespace ICSharpCode.NRefactory.TypeSystem.Implementation
{
	/// <summary>
	/// Represents multiple type resolve contexts.
	/// </summary>
	public class MultiTypeResolveContext : ITypeResolveContext
	{
		readonly ITypeResolveContext[] contexts;
		
		/// <summary>
		/// Creates a new <see cref="MultiTypeResolveContext"/>
		/// </summary>
		public MultiTypeResolveContext(IEnumerable<ITypeResolveContext> contexts)
		{
			if (contexts == null)
				throw new ArgumentNullException("contexts");
			this.contexts = contexts.ToArray();
		}
		
		/// <inheritdoc/>
		public ITypeDefinition GetClass(string fullTypeName, int typeParameterCount, StringComparer nameComparer)
		{
			foreach (ITypeResolveContext context in contexts) {
				ITypeDefinition d = context.GetClass(fullTypeName, typeParameterCount, nameComparer);
				if (d != null)
					return d;
			}
			return null;
		}
		
		/// <inheritdoc/>
		public IEnumerable<ITypeDefinition> GetClasses()
		{
			return contexts.SelectMany(c => c.GetClasses());
		}
		
		/// <inheritdoc/>
		public IEnumerable<ITypeDefinition> GetClasses(string nameSpace, StringComparer nameComparer)
		{
			return contexts.SelectMany(c => c.GetClasses(nameSpace, nameComparer));
		}
		
		/// <inheritdoc/>
		public IEnumerable<string> GetNamespaces()
		{
			return contexts.SelectMany(c => c.GetNamespaces()).Distinct();
		}
		
		/// <inheritdoc/>
		public ISynchronizedTypeResolveContext Synchronize()
		{
			ISynchronizedTypeResolveContext[] sync = new ISynchronizedTypeResolveContext[contexts.Length];
			for (int i = 0; i < sync.Length; i++) {
				sync[i] = contexts[i].Synchronize();
			}
			return new MultiSynchronizedTypeResolveContext(this, sync);
		}
		
		sealed class MultiSynchronizedTypeResolveContext : ProxyTypeResolveContext, ISynchronizedTypeResolveContext
		{
			readonly ISynchronizedTypeResolveContext[] sync;
			
			public MultiSynchronizedTypeResolveContext(ITypeResolveContext target, ISynchronizedTypeResolveContext[] sync)
				: base(target)
			{
				this.sync = sync;
			}
			
			public void Dispose()
			{
				foreach (var element in sync) {
					element.Dispose();
				}
			}
		}
	}
}
