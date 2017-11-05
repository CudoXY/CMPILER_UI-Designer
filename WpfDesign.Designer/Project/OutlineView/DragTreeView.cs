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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ICSharpCode.WpfDesign.Designer.OutlineView
{
	// limitations:
	// - Do not use ItemsSource (use Root)
	// - Do not use Items (use Root)
	public class DragTreeView : TreeView
	{
		static DragTreeView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DragTreeView), 
				new FrameworkPropertyMetadata(typeof(DragTreeView)));
		}

		public DragTreeView()
		{
			AllowDrop = true;
			new DragListener(this).DragStarted += new MouseButtonEventHandler(DragTreeView_DragStarted);
		}

		DragTreeViewItem dropTarget;
		DragTreeViewItem treeItem;
		DragTreeViewItem dropAfter;
		int part;		
		bool dropInside;
		bool dropCopy;
		bool canDrop;

		Border insertLine;

		public static readonly DependencyProperty RootProperty =
			DependencyProperty.Register("Root", typeof(object), typeof(DragTreeView));

		public object Root {
			get { return (object)GetValue(RootProperty); }
			set { SetValue(RootProperty, value); }
		}
		
		#region Filtering
		
		public static readonly DependencyProperty FilterProperty =
			DependencyProperty.Register("Filter", typeof(string), typeof(DragTreeView), new PropertyMetadata(OnFilterPropertyChanged));

		public string Filter {
			get { return (string)GetValue(FilterProperty); }
			set { SetValue(FilterProperty, value); }
		}
		
		private static void OnFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctl = d as DragTreeView;
			var ev = ctl.FilterChanged;
			if (ev != null)
				ev(ctl.Filter);
		}
		
		public event Action<string> FilterChanged;	
		
		public virtual bool ShouldItemBeVisible(DragTreeViewItem dragTreeViewitem)
		{
			return true;
		}

		#endregion
		
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == RootProperty) {
				ItemsSource = new[] { Root };
			}
		}

		void DragTreeView_DragStarted(object sender, MouseButtonEventArgs e)
		{
			DragDrop.DoDragDrop(this, this, DragDropEffects.All);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			insertLine = (Border)Template.FindName("PART_InsertLine", this);
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new DragTreeViewItem();
		}

		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is DragTreeViewItem;
		}

		protected override void OnDragEnter(DragEventArgs e)
		{
			ProcessDrag(e);
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			ProcessDrag(e);
		}

		protected override void OnDrop(DragEventArgs e)
		{
			ProcessDrop(e);
		}

		protected override void OnDragLeave(DragEventArgs e)
		{
			HideDropMarker();
		}

		void PrepareDropInfo(DragEventArgs e)
		{
			dropTarget = null;
			dropAfter = null;
			treeItem = (e.OriginalSource as DependencyObject).GetVisualAncestors().OfType<DragTreeViewItem>().FirstOrDefault();

			if (treeItem != null) {
				var parent = ItemsControl.ItemsControlFromItemContainer(treeItem) as DragTreeViewItem;
				ContentPresenter header = treeItem.HeaderPresenter;
				Point p = e.GetPosition(header);
				part = (int)(p.Y / (header.ActualHeight / 3));
				dropCopy = Keyboard.IsKeyDown(Key.LeftCtrl);
				dropInside = false;

				if (part == 1 || parent == null) {
					dropTarget = treeItem;
					dropInside = true;
					if (treeItem.Items.Count > 0) {
						dropAfter = treeItem.ItemContainerGenerator.ContainerFromIndex(treeItem.Items.Count - 1) as DragTreeViewItem;
					}					
				}
				else if (part == 0) {
					dropTarget = parent;
					var index = dropTarget.ItemContainerGenerator.IndexFromContainer(treeItem);
					if (index > 0) {
						dropAfter = dropTarget.ItemContainerGenerator.ContainerFromIndex(index - 1) as DragTreeViewItem;
					}
				}
				else {
					dropTarget = parent;
					dropAfter = treeItem;
				}
			}
		}

		void ProcessDrag(DragEventArgs e)
		{
			e.Effects = DragDropEffects.None;
			e.Handled = true;
			canDrop = false;

			if (e.Data.GetData(GetType()) != this) return;

			HideDropMarker();
			PrepareDropInfo(e);

			if (dropTarget != null && CanInsertInternal()) {
				canDrop = true;
				e.Effects = dropCopy ? DragDropEffects.Copy : DragDropEffects.Move;
				DrawDropMarker();
			}
		}

		void ProcessDrop(DragEventArgs e)
		{
			HideDropMarker();

			if (canDrop) {
				InsertInternal();
			}
		}

		void DrawDropMarker()
		{
			if (dropInside) {
				dropTarget.IsDragHover = true;
			}
			else {
				var header = treeItem.HeaderPresenter;
				var p = header.TransformToVisual(this).Transform(
					new Point(0, part == 0 ? 0 : header.ActualHeight));

				insertLine.Visibility = Visibility.Visible;
				insertLine.Margin = new Thickness(p.X, p.Y, 0, 0);
			}
		}

		void HideDropMarker()
		{
			insertLine.Visibility = Visibility.Collapsed;
			if (dropTarget != null) {
				dropTarget.IsDragHover = false;
			}
		}

		internal HashSet<DragTreeViewItem> Selection = new HashSet<DragTreeViewItem>();
		DragTreeViewItem upSelection;

		internal void ItemMouseDown(DragTreeViewItem item)
		{
			upSelection = null;
			bool control = Keyboard.IsKeyDown(Key.LeftCtrl);

			if (Selection.Contains(item)) {
				if (control) {
					Unselect(item);
				}
				else {
					upSelection = item;
				}
			}
			else {
				if (control) {
					Select(item);
				}
				else {
					SelectOnly(item);
				}
			}
		}

		internal void ItemMouseUp(DragTreeViewItem item)
		{
			if (upSelection == item) {
				SelectOnly(item);
			}
			upSelection = null;
		}

		internal void ItemAttached(DragTreeViewItem item)
		{
			if (item.IsSelected) Selection.Add(item);
		}

		internal void ItemDetached(DragTreeViewItem item)
		{
			if (item.IsSelected) Selection.Remove(item);
		}

		internal void ItemIsSelectedChanged(DragTreeViewItem item)
		{
			if (item.IsSelected) {
				Selection.Add(item);
			}
			else {
				Selection.Remove(item);
			}
		}

		void Select(DragTreeViewItem item)
		{
			Selection.Add(item);
			item.IsSelected = true;
			OnSelectionChanged();
		}

		void Unselect(DragTreeViewItem item)
		{
			Selection.Remove(item);
			item.IsSelected = false;
			OnSelectionChanged();
		}

		protected virtual void SelectOnly(DragTreeViewItem item)
		{
			ClearSelection();
			Select(item);
			OnSelectionChanged();
		}

		void ClearSelection()
		{
			foreach (var treeItem in Selection.ToArray()) {
				treeItem.IsSelected = false;
			}
			Selection.Clear();
			OnSelectionChanged();
		}

		void OnSelectionChanged()
		{
		}

		bool CanInsertInternal()
		{
			if (!dropCopy) {
				var item = dropTarget;
				while (true) {
					if (Selection.Contains(item)) return false;
					item = ItemsControl.ItemsControlFromItemContainer(item) as DragTreeViewItem;
					if (item == null) break;
				}

				if (Selection.Contains(dropAfter)) return false;
			}

			return CanInsert(dropTarget, Selection.ToArray(), dropAfter, dropCopy);
		}

		void InsertInternal()
		{
			var selection = Selection.ToArray();

			if (!dropCopy) {
				foreach (var item in Selection.ToArray()) {
					var parent = ItemsControl.ItemsControlFromItemContainer(item) as DragTreeViewItem;
					//TODO
					if (parent != null) {
						Remove(parent, item);
					}
				}
			}
			Insert(dropTarget, selection, dropAfter, dropCopy);
		}

		protected virtual bool CanInsert(DragTreeViewItem target, DragTreeViewItem[] items, DragTreeViewItem after, bool copy)
		{
			return true;
		}

		protected virtual void Insert(DragTreeViewItem target, DragTreeViewItem[] items, DragTreeViewItem after, bool copy)
		{
		}

		protected virtual void Remove(DragTreeViewItem target, DragTreeViewItem item)
		{
		}
	}
}
