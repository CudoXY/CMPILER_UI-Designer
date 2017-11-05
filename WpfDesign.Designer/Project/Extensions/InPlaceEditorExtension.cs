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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using ICSharpCode.WpfDesign.Adorners;
using ICSharpCode.WpfDesign.Extensions;
using ICSharpCode.WpfDesign.Designer.Controls;
using ICSharpCode.WpfDesign.UIExtensions;

namespace ICSharpCode.WpfDesign.Designer.Extensions
{
	/// <summary>
	/// Extends In-Place editor to edit any text in the designer which is wrapped in the Visual tree under TexBlock
	/// </summary>
	[ExtensionFor(typeof(TextBlock))]
	public class InPlaceEditorExtension : PrimarySelectionAdornerProvider
	{
		AdornerPanel adornerPanel;
		RelativePlacement placement;
		InPlaceEditor editor;
		/// <summary> Is the element in the Visual tree of the extended element which is being edited. </summary>
		TextBlock textBlock;
		FrameworkElement element;
		DesignPanel designPanel;

		bool isGettingDragged;   // Flag to get/set whether the extended element is dragged.
		bool isMouseDown;        // Flag to get/set whether left-button is down on the element.
		int numClicks;           // No of left-button clicks on the element.

		public InPlaceEditorExtension()
		{
			adornerPanel = new AdornerPanel();
			isGettingDragged = false;
			isMouseDown = Mouse.LeftButton == MouseButtonState.Pressed ? true : false;
			numClicks = 0;
		}

		protected override void OnInitialized()
		{
			base.OnInitialized();
			element = ExtendedItem.Component as FrameworkElement;
			editor = new InPlaceEditor(ExtendedItem);
			editor.DataContext = element;
			editor.Visibility = Visibility.Hidden; // Hide the editor first, It's visibility is governed by mouse events.

			placement = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
			adornerPanel.Children.Add(editor);
			Adorners.Add(adornerPanel);

			designPanel = ExtendedItem.Services.GetService<IDesignPanel>() as DesignPanel;
			Debug.Assert(designPanel != null);

			/* Add mouse event handlers */
			designPanel.PreviewMouseLeftButtonDown += MouseDown;
			designPanel.PreviewMouseLeftButtonUp += MouseUp;
			designPanel.PreviewMouseMove += MouseMove;

			/* To update the position of Editor in case of resize operation */
			ExtendedItem.PropertyChanged += PropertyChanged;
		}

		/// <summary>
		/// Checks whether heigth/width have changed and updates the position of editor
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (textBlock != null)
			{
				if (e.PropertyName == "Width")
				{
					placement.XOffset = Mouse.GetPosition((IInputElement)element).X - Mouse.GetPosition(textBlock).X - 2.8;
					editor.MaxWidth = Math.Max((ModelTools.GetWidth(element) - placement.XOffset), 0);
				}
				if (e.PropertyName == "Height")
				{
					placement.YOffset = Mouse.GetPosition((IInputElement)element).Y - Mouse.GetPosition(textBlock).Y - 1;
					editor.MaxHeight = Math.Max((ModelTools.GetHeight(element) - placement.YOffset), 0);
				}
				AdornerPanel.SetPlacement(editor, placement);
			}
		}

		/// <summary>
		/// Places the handle from a calculated offset using Mouse Positon
		/// </summary>
		/// <param name="text"></param>
		/// <param name="e"></param>
		void PlaceEditor(Visual text, MouseEventArgs e)
		{
			textBlock = text as TextBlock;
			Debug.Assert(textBlock != null);

			/* Gets the offset between the top-left corners of the element and the editor*/
			placement.XOffset = e.GetPosition(element).X - e.GetPosition(textBlock).X - 2.8;
			placement.YOffset = e.GetPosition(element).Y - e.GetPosition(textBlock).Y - 1;
			placement.XRelativeToAdornerWidth = 0;
			placement.XRelativeToContentWidth = 0;
			placement.YRelativeToAdornerHeight = 0;
			placement.YRelativeToContentHeight = 0;

			/* Change data context of the editor to the TextBlock */
			editor.DataContext = textBlock;

			/* Set MaxHeight and MaxWidth so that editor doesn't cross the boundaries of the control */
			editor.SetBinding(FrameworkElement.WidthProperty, new Binding("ActualWidth"));
			editor.SetBinding(FrameworkElement.HeightProperty, new Binding("ActualHeight"));

			/* Hides the TextBlock in control because of some minor offset in placement, overlaping makes text look fuzzy */
			textBlock.Visibility = Visibility.Hidden; // 
			AdornerPanel.SetPlacement(editor, placement);

			RemoveBorder(); // Remove the highlight border.
		}

		/// <summary>
		/// Aborts the editing. This aborts the underlying change group of the editor
		/// </summary>
		public void AbortEdit()
		{
			editor.AbortEditing();
		}

		/// <summary>
		/// Starts editing once again. This aborts the underlying change group of the editor
		/// </summary>
		public void StartEdit()
		{
			editor.StartEditing();
		}

		#region MouseEvents
		DesignPanelHitTestResult result;
		Point Current;
		Point Start;

		void MouseDown(object sender, MouseEventArgs e)
		{
			result = designPanel.HitTest(e.GetPosition(designPanel), false, true, HitTestType.Default);
			if (result.ModelHit == ExtendedItem && result.VisualHit is TextBlock)
			{
				Start = Mouse.GetPosition(null);
				Current = Start;
				isMouseDown = true;
			}
			numClicks++;
		}

		void MouseMove(object sender, MouseEventArgs e)
		{
			Current += e.GetPosition(null) - Start;
			result = designPanel.HitTest(e.GetPosition(designPanel), false, true, HitTestType.Default);
			if (result.ModelHit == ExtendedItem && result.VisualHit is TextBlock)
			{
				if (numClicks > 0)
				{
					if (isMouseDown &&
						((Current - Start).X > SystemParameters.MinimumHorizontalDragDistance
						 || (Current - Start).Y > SystemParameters.MinimumVerticalDragDistance))
					{

						isGettingDragged = true;
						editor.Focus();
					}
				}
				DrawBorder((FrameworkElement)result.VisualHit);
			}
			else {
				RemoveBorder();
			}
		}

		void MouseUp(object sender, MouseEventArgs e)
		{
			result = designPanel.HitTest(e.GetPosition(designPanel), true, true, HitTestType.Default);
			if (((result.ModelHit == ExtendedItem && result.VisualHit is TextBlock) || (result.VisualHit != null && result.VisualHit.TryFindParent<InPlaceEditor>() == editor)) && numClicks > 0)
			{
				if (!isGettingDragged) {
					PlaceEditor(ExtendedItem.View, e);
					foreach (var extension in ExtendedItem.Extensions)
					{
						if (!(extension is InPlaceEditorExtension) && !(extension is SelectedElementRectangleExtension)) {
							ExtendedItem.RemoveExtension(extension);
						}
					}
					editor.Visibility = Visibility.Visible;
				}
			}
			else { // Clicked outside the Text - > hide the editor and make the actualt text visible again
				editor.Visibility = Visibility.Hidden;
				if (textBlock != null) {
					textBlock.Visibility = Visibility.Visible;
				}
				this.ExtendedItem.ReapplyAllExtensions();
			}

			isMouseDown = false;
			isGettingDragged = false;
		}

		#endregion

		#region HighlightBorder
		private Border _border;
		private sealed class BorderPlacement : AdornerPlacement
		{
			private readonly FrameworkElement _element;

			public BorderPlacement(FrameworkElement element)
			{
				_element = element;
			}

			public override void Arrange(AdornerPanel panel, UIElement adorner, Size adornedElementSize)
			{
				Point p = _element.TranslatePoint(new Point(), panel.AdornedElement);
				var rect = new Rect(p, _element.RenderSize);
				rect.Inflate(3, 1);
				adorner.Arrange(rect);
			}
		}

		private void DrawBorder(FrameworkElement item)
		{
			if (editor != null && editor.Visibility != Visibility.Visible)
			{
				if (adornerPanel.Children.Contains(_border))
					adornerPanel.Children.Remove(_border);
				_border = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1.4), ToolTip = "Edit this Text", SnapsToDevicePixels = true };
				var shadow = new DropShadowEffect { Color = Colors.LightGray, ShadowDepth = 3 };
				_border.Effect = shadow;
				var bp = new BorderPlacement(item);
				AdornerPanel.SetPlacement(_border, bp);
				adornerPanel.Children.Add(_border);
			}
		}

		private void RemoveBorder()
		{
			if (adornerPanel.Children.Contains(_border))
				adornerPanel.Children.Remove(_border);
		}
		#endregion

		protected override void OnRemove()
		{
			if (textBlock != null) {
				textBlock.Visibility = Visibility.Visible;
			}
			ExtendedItem.PropertyChanged -= PropertyChanged;
			designPanel.PreviewMouseLeftButtonDown -= MouseDown;
			designPanel.PreviewMouseMove -= MouseMove;
			designPanel.PreviewMouseLeftButtonUp -= MouseUp;
			base.OnRemove();
		}
	}
}
