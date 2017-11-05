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
using System.Windows;
using System.Windows.Input;
using ICSharpCode.WpfDesign.Designer.themes;

namespace ICSharpCode.WpfDesign.Designer.OutlineView
{
	public partial class Outline
	{
		public Outline()
		{
			SpecialInitializeComponent();

			this.AddCommandHandler(ApplicationCommands.Undo,
				() => ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.Undo(),
				() => Root == null ? false : ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.CanUndo());
			this.AddCommandHandler(ApplicationCommands.Redo,
				() => ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.Redo(),
				() => Root == null ? false : ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.CanRedo());
			this.AddCommandHandler(ApplicationCommands.Copy,
				() => ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.Copy(),
				() => Root == null ? false : ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.CanCopyOrCut());
			this.AddCommandHandler(ApplicationCommands.Cut,
				() => ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.Cut(),
				() => Root == null ? false : ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.CanCopyOrCut());
			this.AddCommandHandler(ApplicationCommands.Delete,
				() => ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.Delete(),
				() => Root == null ? false : ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.CanDelete());
			this.AddCommandHandler(ApplicationCommands.Paste,
				() => ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.Paste(),
				() => Root == null ? false : ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.CanPaste());
			this.AddCommandHandler(ApplicationCommands.SelectAll,
				() => ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.SelectAll(),
				() => Root == null ? false : ((DesignPanel) Root.DesignItem.Services.DesignPanel).DesignSurface.CanSelectAll());
		}

		/// <summary>
		/// Fixes InitializeComponent with multiple Versions of same Assembly loaded
		/// </summary>
		public void SpecialInitializeComponent()
		{
			if (!this._contentLoaded) {
				this._contentLoaded = true;
				Uri resourceLocator = new Uri(VersionedAssemblyResourceDictionary.GetXamlNameForType(this.GetType()), UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
			
			this.InitializeComponent();
		}

		public static readonly DependencyProperty RootProperty =
			DependencyProperty.Register("Root", typeof(IOutlineNode), typeof(Outline));

		public IOutlineNode Root
		{
			get { return (IOutlineNode)GetValue(RootProperty); }
			set { SetValue(RootProperty, value); }
		}
		
		public object OutlineContent {
			get { return this; }
		}
	}
}
