﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;

namespace ICSharpCode.WpfDesign.Extensions
{
	/// <summary>
	/// Combines two extension servers using a logical OR.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
	public sealed class LogicalOrExtensionServer<A, B> : DefaultExtensionServer
		where A : ExtensionServer
		where B : ExtensionServer
	{
		ExtensionServer _a;
		ExtensionServer _b;
		
		/// <summary/>
		protected override void OnInitialized()
		{
			base.OnInitialized();
			_a = Context.Services.ExtensionManager.GetExtensionServer(new ExtensionServerAttribute(typeof(A)));
			_b = Context.Services.ExtensionManager.GetExtensionServer(new ExtensionServerAttribute(typeof(B)));
			_a.ShouldApplyExtensionsInvalidated += OnShouldApplyExtensionsInvalidated;
			_b.ShouldApplyExtensionsInvalidated += OnShouldApplyExtensionsInvalidated;
		}
		
		void OnShouldApplyExtensionsInvalidated(object sender, DesignItemCollectionEventArgs e)
		{
			ReapplyExtensions(e.Items);
		}
		
		/// <summary/>
		public override bool ShouldApplyExtensions(DesignItem extendedItem)
		{
			return _a.ShouldApplyExtensions(extendedItem) || _b.ShouldApplyExtensions(extendedItem);
		}
	}
	
	/// <summary>
	/// Combines two extension servers using a logical AND.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
	public sealed class LogicalAndExtensionServer<A, B> : DefaultExtensionServer
		where A : ExtensionServer
		where B : ExtensionServer
	{
		ExtensionServer _a;
		ExtensionServer _b;
		
		/// <summary/>
		protected override void OnInitialized()
		{
			base.OnInitialized();
			_a = Context.Services.ExtensionManager.GetExtensionServer(new ExtensionServerAttribute(typeof(A)));
			_b = Context.Services.ExtensionManager.GetExtensionServer(new ExtensionServerAttribute(typeof(B)));
			_a.ShouldApplyExtensionsInvalidated += OnShouldApplyExtensionsInvalidated;
			_b.ShouldApplyExtensionsInvalidated += OnShouldApplyExtensionsInvalidated;
		}
		
		void OnShouldApplyExtensionsInvalidated(object sender, DesignItemCollectionEventArgs e)
		{
			ReapplyExtensions(e.Items);
		}
		
		/// <summary/>
		public override bool ShouldApplyExtensions(DesignItem extendedItem)
		{
			return _a.ShouldApplyExtensions(extendedItem) && _b.ShouldApplyExtensions(extendedItem);
		}
	}
}
