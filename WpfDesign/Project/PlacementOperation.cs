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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ICSharpCode.WpfDesign
{
	/// <summary>
	/// Stores data about a placement operation.
	/// </summary>
	public sealed class PlacementOperation
	{
		/// <summary>
		/// A exception wich can Happen during Placement
		/// </summary>
		public class PlacementOperationException : InvalidOperationException
		{
			/// <summary>
			/// Constructor for Placement Exception
			/// </summary>
			/// <param name="message"></param>
			public PlacementOperationException(string message)
				: base(message)
			{ }
		}

		readonly ChangeGroup changeGroup;
		readonly ReadOnlyCollection<PlacementInformation> placedItems;
		readonly PlacementType type;
		DesignItem currentContainer;
		IPlacementBehavior currentContainerBehavior;
		bool isAborted, isCommitted;
		
		/// <summary>
		/// Offset for inserted Components
		/// </summary>
		public const double PasteOffset = 10;
		
		#region Properties
		/// <summary>
		/// The items being placed.
		/// </summary>
		public ReadOnlyCollection<PlacementInformation> PlacedItems {
			get { return placedItems; }
		}
		
		/// <summary>
		/// The type of the placement being done.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
		public PlacementType Type {
			get { return type; }
		}
		
		/// <summary>
		/// Gets if the placement operation was aborted.
		/// </summary>
		public bool IsAborted {
			get { return isAborted; }
		}
		
		/// <summary>
		/// Gets if the placement operation was committed.
		/// </summary>
		public bool IsCommitted {
			get { return isCommitted; }
		}
		
		/// <summary>
		/// Gets the current container for the placement operation.
		/// </summary>
		public DesignItem CurrentContainer {
			get { return currentContainer; }
		}
		
		/// <summary>
		/// Gets the placement behavior for the current container.
		/// </summary>
		public IPlacementBehavior CurrentContainerBehavior {
			get { return currentContainerBehavior; }
		}
		
		#endregion
		
		#region Container changing
		/// <summary>
		/// Make the placed item switch the container.
		/// This method assumes that you already have checked if changing the container is possible.
		/// </summary>
		public void ChangeContainer(DesignItem newContainer)
		{
			if (newContainer == null)
				throw new ArgumentNullException("newContainer");
			if (isAborted || isCommitted)
				throw new PlacementOperationException("The operation is not running anymore.");
			if (currentContainer == newContainer)
				return;
			
			if (!currentContainerBehavior.CanLeaveContainer(this))
				throw new PlacementOperationException("The items cannot be removed from their parent container.");
			
			try {
				currentContainerBehavior.LeaveContainer(this);
				
				GeneralTransform transform = currentContainer.View.TransformToVisual(newContainer.View);
				
				foreach (PlacementInformation info in placedItems) {
					info.OriginalBounds = TransformRectByMiddlePoint(transform, info.OriginalBounds);
					info.Bounds = TransformRectByMiddlePoint(transform, info.Bounds).Round();
				}
				
				currentContainer = newContainer;
				currentContainerBehavior = newContainer.GetBehavior<IPlacementBehavior>();
				
				Debug.Assert(currentContainerBehavior != null);
				currentContainerBehavior.EnterContainer(this);
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				Abort();
				throw;
			}
		}
		
		static Rect TransformRectByMiddlePoint(GeneralTransform transform, Rect r)
		{
			// we don't want to adjust the size of the control when moving it out of a scaled
			// container, we just want to move it correcly
			Point p = new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
			Vector movement = transform.Transform(p) - p;
			return new Rect(r.TopLeft + movement, r.Size);
		}
		#endregion
		
		#region Delete Items
		/// <summary>
		/// Deletes the items being placed, and commits the placement operation.
		/// </summary>
		public void DeleteItemsAndCommit()
		{
			if (isAborted || isCommitted)
				throw new PlacementOperationException("The operation is not running anymore.");
			if (!currentContainerBehavior.CanLeaveContainer(this))
				throw new PlacementOperationException("The items cannot be removed from their parent container.");
			
			currentContainerBehavior.LeaveContainer(this);
			Commit();
		}
		#endregion
		
		#region Start
		/// <summary>
		/// Starts a new placement operation that changes the placement of <paramref name="placedItems"/>.
		/// </summary>
		/// <param name="placedItems">The items to be placed.</param>
		/// <param name="type">The type of the placement.</param>
		/// <returns>A PlacementOperation object.</returns>
		/// <remarks>
		/// You MUST call either <see cref="Abort"/> or <see cref="Commit"/> on the returned PlacementOperation
		/// once you are done with it, otherwise a ChangeGroup will be left open and Undo/Redo will fail to work!
		/// </remarks>
		public static PlacementOperation Start(ICollection<DesignItem> placedItems, PlacementType type)
		{
			if (placedItems == null)
				throw new ArgumentNullException("placedItems");
			if (type == null)
				throw new ArgumentNullException("type");
			DesignItem[] items = placedItems.ToArray();
			if (items.Length == 0)
				throw new ArgumentException("placedItems.Length must be > 0");
			
			PlacementOperation op = new PlacementOperation(items, type);
			try {
				if (op.currentContainerBehavior == null)
					throw new PlacementOperationException("Starting the operation is not supported");
				
				op.currentContainerBehavior.BeginPlacement(op);
				foreach (PlacementInformation info in op.placedItems) {
					info.OriginalBounds = op.currentContainerBehavior.GetPosition(op, info.Item);
					info.Bounds = info.OriginalBounds;
				}
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				op.changeGroup.Abort();
				throw;
			}
			return op;
		}
		private PlacementOperation(DesignItem[] items, PlacementType type)
		{
			List<DesignItem> moveableItems;
			this.currentContainerBehavior = GetPlacementBehavior(items, out moveableItems, type);

			PlacementInformation[] information = new PlacementInformation[moveableItems.Count];
			for (int i = 0; i < information.Length; i++) {
				information[i] = new PlacementInformation(moveableItems[i], this);
			}
			this.placedItems = new ReadOnlyCollection<PlacementInformation>(information);
			this.type = type;
			
			this.currentContainer = moveableItems[0].Parent;
			
			this.changeGroup = moveableItems[0].Context.OpenGroup(type.ToString(), moveableItems);
		}

		/// <summary>
		/// The Size wich the Element really should have (even if its smaller Rendered (like emtpy Image!))
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static Size GetRealElementSize(UIElement element)
		{
			var size = element.RenderSize;
			if (element is FrameworkElement && !double.IsNaN(((FrameworkElement)element).Width))
				size.Width = ((FrameworkElement)element).Width;
			if (element is FrameworkElement && !double.IsNaN(((FrameworkElement)element).Height))
				size.Height = ((FrameworkElement)element).Height;

			if (element is FrameworkElement && size.Width < ((FrameworkElement)element).MinWidth)
				size.Width = ((FrameworkElement)element).MinWidth;
			if (element is FrameworkElement && size.Height < ((FrameworkElement)element).MinHeight)
				size.Height = ((FrameworkElement)element).MinHeight;

			return size;
		}

		/// <summary>
		/// Gets the placement behavior associated with the specified items.
		/// </summary>
		public static IPlacementBehavior GetPlacementBehavior(ICollection<DesignItem> items)
		{
			List<DesignItem> moveableItems;
			return GetPlacementBehavior(items, out moveableItems, PlacementType.Move);
		}

		/// <summary>
		/// Gets the placement behavior associated with the specified items.
		/// </summary>
		public static IPlacementBehavior GetPlacementBehavior(ICollection<DesignItem> items, out List<DesignItem> moveableItems, PlacementType placementType)
		{
			moveableItems = new List<DesignItem>();
			
			if (items == null)
				throw new ArgumentNullException("items");
			if (items.Count == 0)
				return null;

			var possibleItems = items;
			if (!items.Any(x => x.Parent == null))
			{
				var itemsPartentGroup = items.GroupBy(x => x.Parent);
				var parents = itemsPartentGroup.Select(x => x.Key).OrderBy(x => x.DepthLevel).First();
				possibleItems = itemsPartentGroup.Where(x => x.Key.DepthLevel == parents.DepthLevel).SelectMany(x => x).ToList();
			}
			
			var first = possibleItems.First();
			DesignItem parent = first.Parent;
			moveableItems.Add(first);
			foreach (DesignItem item in possibleItems.Skip(1))
			{
				if (item.Parent != parent) {
					if (placementType != PlacementType.MoveAndIgnoreOtherContainers) {
						return null;
					}
				}
				else
					moveableItems.Add(item);
			}
			if (parent != null)
				return parent.GetBehavior<IPlacementBehavior>();
			else if (possibleItems.Count == 1)
				return first.GetBehavior<IRootPlacementBehavior>();
			else
				return null;
		}
		#endregion

		#region StartInsertNewComponents
		/// <summary>
		/// Try to insert new components into the container.
		/// </summary>
		/// <param name="container">The container that should become the parent of the components.</param>
		/// <param name="placedItems">The components to add to the container.</param>
		/// <param name="positions">The rectangle specifying the position the element should get.</param>
		/// <param name="type">The type </param>
		/// <returns>The operation that inserts the new components, or null if inserting is not possible.</returns>
		public static PlacementOperation TryStartInsertNewComponents(DesignItem container, IList<DesignItem> placedItems, IList<Rect> positions, PlacementType type)
		{
			if (container == null)
				throw new ArgumentNullException("container");
			if (placedItems == null)
				throw new ArgumentNullException("placedItems");
			if (positions == null)
				throw new ArgumentNullException("positions");
			if (type == null)
				throw new ArgumentNullException("type");
			if (placedItems.Count == 0)
				throw new ArgumentException("placedItems.Count must be > 0");
			if (placedItems.Count != positions.Count)
				throw new ArgumentException("positions.Count must be = placedItems.Count");
			
			DesignItem[] items = placedItems.ToArray();
			
			PlacementOperation op = new PlacementOperation(items, type);
			try {
				for (int i = 0; i < items.Length; i++) {
					op.placedItems[i].OriginalBounds = op.placedItems[i].Bounds = positions[i];
				}
				op.currentContainer = container;
				op.currentContainerBehavior = container.GetBehavior<IPlacementBehavior>();
				if (op.currentContainerBehavior == null || !op.currentContainerBehavior.CanEnterContainer(op, true)) {
					op.changeGroup.Abort();
					return null;
				}
				op.currentContainerBehavior.EnterContainer(op);
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				op.changeGroup.Abort();
				throw;
			}
			return op;
		}
		#endregion
		
		#region ChangeGroup handling
		
		/// <summary>
		/// Gets/Sets the description of the underlying change group.
		/// </summary>
		public string Description {
			get { return changeGroup.Title; }
			set { changeGroup.Title = value; }
		}
			
		/// <summary>
		/// Aborts the operation.
		/// This aborts the underlying change group, reverting all changes done while the operation was running.
		/// </summary>
		public void Abort()
		{
			if (!isAborted) {
				if (isCommitted)
					throw new PlacementOperationException("PlacementOperation is committed.");
				isAborted = true;
				currentContainerBehavior.EndPlacement(this);
				changeGroup.Abort();
			}
		}
		
		/// <summary>
		/// Commits the operation.
		/// This commits the underlying change group.
		/// </summary>
		public void Commit()
		{
			if (isAborted || isCommitted)
				throw new PlacementOperationException("PlacementOperation is already aborted/committed.");
			isCommitted = true;
			currentContainerBehavior.EndPlacement(this);
			changeGroup.Commit();
		}
		#endregion
	}
}
