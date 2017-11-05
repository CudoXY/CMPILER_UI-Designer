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
using System.Windows;
using ICSharpCode.WpfDesign.Extensions;
using ICSharpCode.WpfDesign.Adorners;
using ICSharpCode.WpfDesign.Designer.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using ICSharpCode.WpfDesign.UIExtensions;
namespace ICSharpCode.WpfDesign.Designer.Extensions
{
	/// <summary>
	/// Description of UserControlPointsObjectExtension.
	/// </summary>
	//[ExtensionFor(typeof(Line), OverrideExtensions = new Type[] { typeof(ResizeThumbExtension), typeof(SelectedElementRectangleExtension), typeof(CanvasPositionExtension), typeof(QuickOperationMenuExtension), typeof(RotateThumbExtension), typeof(RenderTransformOriginExtension), typeof(InPlaceEditorExtension), typeof(SkewThumbExtension) })]
	public abstract class UserControlPointsObjectExtension : LineExtensionBase
	{
		//
		private double CurrentX2;
		private double CurrentY2;
		private double CurrentLeft;
		private double CurrentTop;

		private IEnumerable<DependencyProperty> _thumbProperties;

		//Size oldSize;
		ZoomControl zoom;

		public DragListener DragListener { get; private set; }

		protected UserControlPointsObjectThumb CreateThumb(PlacementAlignment alignment, Cursor cursor, DependencyProperty property)
		{
			var designerThumb = new UserControlPointsObjectThumb { Alignment = alignment, Cursor = cursor, IsPrimarySelection = true, DependencyProperty = property };
			AdornerPanel.SetPlacement(designerThumb, Place(designerThumb, alignment));

			adornerPanel.Children.Add(designerThumb);

			DragListener = new DragListener(designerThumb);
			DragListener.Started += drag_Started;
			DragListener.Changed += drag_Changed;
			DragListener.Completed += drag_Completed;

			return designerThumb;
		}

		#region eventhandlers

		protected virtual void drag_Started(DragListener drag)
		{
			Line al = ExtendedItem.View as Line;
			CurrentX2 = al.X2;
			CurrentY2 = al.Y2;
			CurrentLeft = (double)al.GetValue(Canvas.LeftProperty);
			CurrentTop = (double)al.GetValue(Canvas.TopProperty);

			var designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
			zoom = designPanel.TryFindParent<ZoomControl>();

			if (resizeBehavior != null)
				operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
			else
			{
				changeGroup = this.ExtendedItem.Context.OpenGroup("Resize", extendedItemArray);
			}
			_isResizing = true;

			(drag.Target as UserControlPointsObjectThumb).IsPrimarySelection = false;
		}

		protected virtual void drag_Changed(DragListener drag)
		{
			Line al = ExtendedItem.View as Line;

			var thumb = drag.Target as UserControlPointsObjectThumb;
			var alignment = thumb.Alignment;
			var info = operation.PlacedItems[0];
			double dx = 0;
			double dy = 0;

			if (zoom != null)
			{
				dx = drag.Delta.X * (1 / zoom.CurrentZoom);
				dy = drag.Delta.Y * (1 / zoom.CurrentZoom);
			}

			double top, left, x, y, xtop, xleft;

			if (alignment == PlacementAlignment.TopLeft)
			{

				//normal values
				x = CurrentX2 - dx;
				y = CurrentY2 - dy;
				top = CurrentTop + dy;
				left = CurrentLeft + dx;

				//values to use when keys are pressed
				xtop = CurrentTop + CurrentY2;
				xleft = CurrentLeft + CurrentX2;

			}
			else
			{
				x = CurrentX2 + dx;
				y = CurrentY2 + dy;
				top = xtop = CurrentTop;
				left = xleft = CurrentLeft;
			}

			//			Bounds position = CalculateDrawing(x, y, left, top, xleft, xtop);
			ExtendedItem.Properties.GetProperty(thumb.DependencyProperty).SetValue(new Point(x, y));
			//			if (operation != null) {
			//				var result = info.OriginalBounds;
			//				result.X = position.Left;
			//				result.Y = position.Top;
			//				result.Width = Math.Abs(position.X);
			//				result.Height = Math.Abs(position.Y);
			//
			//				info.Bounds = result.Round();
			//				operation.CurrentContainerBehavior.BeforeSetPosition(operation);
			//				operation.CurrentContainerBehavior.SetPosition(info);
			//			}

			(drag.Target as UserControlPointsObjectThumb).InvalidateArrange();
			ResetWidthHeightProperties();
		}

		protected virtual void drag_Completed(DragListener drag)
		{
			if (operation != null)
			{
				if (drag.IsCanceled) operation.Abort();
				else
				{
					ResetWidthHeightProperties();

					operation.Commit();
				}
				operation = null;
			}
			else
			{
				if (drag.IsCanceled)
					changeGroup.Abort();
				else
					changeGroup.Commit();
				changeGroup = null;
			}

			_isResizing = false;
			(drag.Target as UserControlPointsObjectThumb).IsPrimarySelection = true;
			HideSizeAndShowHandles();
		}

		#endregion

		/// <summary>
		/// is invoked whenever a line is selected on the canvas, remember that the adorners are created for each line object and never destroyed
		/// </summary>
		protected override void OnInitialized()
		{
			base.OnInitialized();

			_thumbProperties = FillThumbProperties();
			var thumbs = new List<UserControlPointsObjectThumb>();
			foreach (var prp in _thumbProperties)
			{
				thumbs.Add(CreateThumb(PlacementAlignment.Center, Cursors.Cross, prp));
			}

			resizeThumbs = thumbs;

			extendedItemArray[0] = this.ExtendedItem;

			Invalidate();

			this.ExtendedItem.PropertyChanged += OnPropertyChanged;
			resizeBehavior = PlacementOperation.GetPlacementBehavior(extendedItemArray);
			UpdateAdornerVisibility();
		}

		protected abstract IEnumerable<DependencyProperty> FillThumbProperties();

		protected virtual Bounds CalculateDrawing(double x, double y, double left, double top, double xleft, double xtop)
		{

			Double theta = (180 / Math.PI) * Math.Atan2(y, x);
			double verticaloffset = Math.Abs(90 - Math.Abs(theta));
			if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
			{
				if (Math.Abs(theta) < 45 || Math.Abs(theta) > 135)
				{
					y = 0;
					top = xtop;
				}
				else if (verticaloffset < 45)
				{
					x = 0;
					left = xleft;
				}
			}
			else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
			{
				if (verticaloffset < 10)
				{
					x = 0;
					left = xleft;
				}
				else if (Math.Abs(theta) < 10 || Math.Abs(theta) > 170)
				{
					y = 0;
					top = xtop;
				}
			}

			SetSurfaceInfo(0, 3, Math.Round((180 / Math.PI) * Math.Atan2(y, x), 0).ToString());
			return new Bounds { X = Math.Round(x, 1), Y = Math.Round(y, 1), Left = Math.Round(left, 1), Top = Math.Round(top, 1) };
		}
	}
}
