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
using System.Linq;
using ICSharpCode.WpfDesign.Designer;

namespace ICSharpCode.WpfDesign.Tests.Designer
{
	internal class MockFocusNavigator
	{
		private readonly DesignContext _context;

		internal MockFocusNavigator(DesignContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Moves the Focus down the tree.
		/// </summary>
		internal void MoveFocusForward()
		{
			ISelectionService selection = _context.Services.Selection;
			DesignItem item = selection.PrimarySelection;
			selection.SetSelectedComponents(selection.SelectedItems, SelectionTypes.Remove);
			if (item != GetLastElement()) {
				if (item.ContentProperty != null) {
					if (item.ContentProperty.IsCollection) {
						if (item.ContentProperty.CollectionElements.Count != 0) {
							if (ModelTools.CanSelectComponent(item.ContentProperty.CollectionElements.First()))
								selection.SetSelectedComponents(new DesignItem[] {item.ContentProperty.CollectionElements.First()}, SelectionTypes.Primary);
							else
								SelectNextInPeers(item);
						} else
							SelectNextInPeers(item);
					} else if (item.ContentProperty.Value != null) {
						if (ModelTools.CanSelectComponent(item.ContentProperty.Value))
							selection.SetSelectedComponents(new DesignItem[] {item.ContentProperty.Value}, SelectionTypes.Primary);
						else
							SelectNextInPeers(item);
					} else {
						SelectNextInPeers(item);
					}
				} else {
					SelectNextInPeers(item);
				}
			} else {
				//if the element was last element move focus to the root element to keep a focus cycle.
				selection.SetSelectedComponents(new DesignItem[] {_context.RootItem}, SelectionTypes.Primary);
			}
		}

		/// <summary>
		/// Moves focus up-the-tree.
		/// </summary>
		internal void MoveFocusBack()
		{
			ISelectionService selection = _context.Services.Selection;
			DesignItem item = selection.PrimarySelection;
			if (item != _context.RootItem) {
				if (item.Parent != null && item.Parent.ContentProperty.IsCollection) {
					int index = item.Parent.ContentProperty.CollectionElements.IndexOf(item);
					if (index != 0) {
						if (ModelTools.CanSelectComponent(item.Parent.ContentProperty.CollectionElements.ElementAt(index - 1)))
							selection.SetSelectedComponents(new DesignItem[] {item.Parent.ContentProperty.CollectionElements.ElementAt(index - 1)}, SelectionTypes.Primary);
					} else {
						if (ModelTools.CanSelectComponent(item.Parent))
							selection.SetSelectedComponents(new DesignItem[] {item.Parent}, SelectionTypes.Primary);
					}
				} else {
					if (ModelTools.CanSelectComponent(item.Parent))
						selection.SetSelectedComponents(new DesignItem[] {item.Parent}, SelectionTypes.Primary);
				}
			} else {
				// if the element was root item move focus again to the last element.
				selection.SetSelectedComponents(new DesignItem[] {GetLastElement()}, SelectionTypes.Primary);
			}
		}

		/// <summary>
		/// Gets the last element in the element hierarchy.
		/// </summary>
		private DesignItem GetLastElement()
		{
			DesignItem item = _context.RootItem;
			while (item != null && item.ContentProperty != null) {
				if (item.ContentProperty.IsCollection) {
					if (item.ContentProperty.CollectionElements.Count != 0) {
						if (ModelTools.CanSelectComponent(item.ContentProperty.CollectionElements.Last()))
							item = item.ContentProperty.CollectionElements.Last();
						else
							break;
					} else
						break;
				} else {
					if (item.ContentProperty.Value != null)
						item = item.ContentProperty.Value;
					else
						break;
				}
			}
			return item;
		}

		/// <summary>
		/// Select the next element in the element collection if <paramref name="item"/> parent's had it's content property as collection.
		/// </summary>
		private void SelectNextInPeers(DesignItem item)
		{
			ISelectionService selection = _context.Services.Selection;
			if (item.Parent != null && item.Parent.ContentProperty != null) {
				if (item.Parent.ContentProperty.IsCollection) {
					int index = item.Parent.ContentProperty.CollectionElements.IndexOf(item);
					if (index != item.Parent.ContentProperty.CollectionElements.Count)
						selection.SetSelectedComponents(new DesignItem[] {item.Parent.ContentProperty.CollectionElements.ElementAt(index + 1)}, SelectionTypes.Primary);
				}
			}
		}
	}
}
