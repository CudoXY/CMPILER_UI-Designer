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

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.WpfDesign.Adorners;
using ICSharpCode.WpfDesign.Designer.Services;

namespace ICSharpCode.WpfDesign.Designer.Controls
{
	/// <summary>
	/// A Info text area.
	/// </summary>
	public sealed class InfoTextEnterArea : FrameworkElement
	{
		Geometry activeAreaGeometry;
		AdornerPanel adornerPanel;
		IDesignPanel designPanel;
		
		public InfoTextEnterArea()
		{
			this.IsHitTestVisible = false;
		}		
			
		public Geometry ActiveAreaGeometry {
			get { return activeAreaGeometry; }
			set {
				activeAreaGeometry = value;
			}
		}	
		
		public static void Start(ref InfoTextEnterArea grayOut, ServiceContainer services, UIElement activeContainer, string text)
		{
			Debug.Assert(activeContainer != null);
			Start(ref grayOut, services, activeContainer, new Rect(activeContainer.RenderSize), text);
		}
		
		public static void Start(ref InfoTextEnterArea grayOut, ServiceContainer services, UIElement activeContainer, Rect activeRectInActiveContainer, string text)
		{
			Debug.Assert(services != null);
			Debug.Assert(activeContainer != null);
			DesignPanel designPanel = services.GetService<IDesignPanel>() as DesignPanel;
			OptionService optionService = services.GetService<OptionService>();
			if (designPanel != null && grayOut == null && optionService != null && optionService.GrayOutDesignSurfaceExceptParentContainerWhenDragging) {
				grayOut = new InfoTextEnterArea();
				grayOut.designPanel = designPanel;
				grayOut.adornerPanel = new AdornerPanel();
				grayOut.adornerPanel.Order = AdornerOrder.Background;
				grayOut.adornerPanel.SetAdornedElement(designPanel.Context.RootItem.View, null);
				grayOut.ActiveAreaGeometry = new RectangleGeometry(activeRectInActiveContainer, 0, 0, (Transform)activeContainer.TransformToVisual(grayOut.adornerPanel.AdornedElement));
				var tb = new TextBlock(){Text = text};
				tb.FontSize = 10;
				tb.ClipToBounds = true;
				tb.Width = ((FrameworkElement) activeContainer).ActualWidth;
				tb.Height = ((FrameworkElement) activeContainer).ActualHeight;
				tb.VerticalAlignment = VerticalAlignment.Top;
				tb.HorizontalAlignment = HorizontalAlignment.Left;
				tb.RenderTransform = (Transform)activeContainer.TransformToVisual(grayOut.adornerPanel.AdornedElement);
				grayOut.adornerPanel.Children.Add(tb);
								
				designPanel.Adorners.Add(grayOut.adornerPanel);
			}
		}
																		 
		public static void Stop(ref InfoTextEnterArea grayOut)
		{
			if (grayOut != null) {
				IDesignPanel designPanel = grayOut.designPanel;
				AdornerPanel adornerPanelToRemove = grayOut.adornerPanel;
				designPanel.Adorners.Remove(adornerPanelToRemove);								
				grayOut = null;
			}
		}
	}
}
