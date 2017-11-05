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
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ICSharpCode.WpfDesign.UIExtensions;

namespace ICSharpCode.WpfDesign.Designer.Controls
{
	public class ZoomButtons : RangeBase
	{
		static ZoomButtons()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomButtons),
			                                         new FrameworkPropertyMetadata(typeof(ZoomButtons)));
		}
		
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			
			var uxPlus = (ButtonBase)Template.FindName("uxPlus", this);
			var uxMinus = (ButtonBase)Template.FindName("uxMinus", this);
			var uxReset = (ButtonBase)Template.FindName("uxReset", this);
			var ux100Percent = (ButtonBase)Template.FindName("ux100Percent", this);

			if (uxPlus != null)
				uxPlus.Click += OnZoomInClick;
			if (uxMinus != null)
				uxMinus.Click += OnZoomOutClick;
			if (uxReset != null)
				uxReset.Click += OnResetClick;
			if (ux100Percent != null)
				ux100Percent.Click += On100PercentClick;
		}
		
		const double ZoomFactor = 1.1;
		
		void OnZoomInClick(object sender, EventArgs e)
		{
			SetCurrentValue(ValueProperty, ZoomScrollViewer.RoundToOneIfClose(this.Value * ZoomFactor));
		}
		
		void OnZoomOutClick(object sender, EventArgs e)
		{
			SetCurrentValue(ValueProperty, ZoomScrollViewer.RoundToOneIfClose(this.Value / ZoomFactor));
		}
		
		void OnResetClick(object sender, EventArgs e)
		{
			SetCurrentValue(ValueProperty, 1.0);
		}
		void On100PercentClick(object sender, EventArgs e)
		{
			var zctl = this.TryFindParent<ZoomControl>();
			var contentWidth = ((FrameworkElement) zctl.Content).ActualWidth;
			var contentHeight = ((FrameworkElement)zctl.Content).ActualHeight;
			var width = zctl.ActualWidth;
			var height = zctl.ActualHeight;

			if (contentWidth > width || contentHeight > height)
			{
				double widthProportion = contentWidth / width;
				double heightProportion = contentHeight / height;

				if (widthProportion > heightProportion)
					SetCurrentValue(ValueProperty, (width - 20.00) / contentWidth);
				else
					SetCurrentValue(ValueProperty, (height - 20.00) / contentHeight);
			}			
		}
	}
}
