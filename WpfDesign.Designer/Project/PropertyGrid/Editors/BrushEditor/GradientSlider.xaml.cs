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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using ICSharpCode.WpfDesign.Designer.themes;

namespace ICSharpCode.WpfDesign.Designer.PropertyGrid.Editors.BrushEditor
{
	public partial class GradientSlider
	{
		public GradientSlider()
		{
			SpecialInitializeComponent();

			BindingOperations.SetBinding(this, SelectedStopProperty, new Binding("SelectedItem") {
			                             	Source = itemsControl,
			                             	Mode = BindingMode.TwoWay
			                             });

			strip.DragStarted += strip_DragStarted;
			strip.DragDelta += strip_DragDelta;
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
		static GradientSlider()
		{
			EventManager.RegisterClassHandler(typeof(GradientSlider),
			                                  Thumb.DragDeltaEvent, new DragDeltaEventHandler(ThumbDragDelta));
            EventManager.RegisterClassHandler(typeof(GradientSlider),
                                              Thumb.DragStartedEvent, new DragStartedEventHandler(ThumbDragStarted));
            EventManager.RegisterClassHandler(typeof(GradientSlider),
                                              Thumb.DragCompletedEvent, new DragCompletedEventHandler(ThumbDragCompleted));
        }

		GradientStop newStop;
		double startOffset;
        static bool isThumbDragInProgress = false;

		public static readonly DependencyProperty BrushProperty =
			DependencyProperty.Register("Brush", typeof(GradientBrush), typeof(GradientSlider));

		public GradientBrush Brush {
			get { return (GradientBrush)GetValue(BrushProperty); }
			set { SetValue(BrushProperty, value); }
		}

		public static readonly DependencyProperty SelectedStopProperty =
			DependencyProperty.Register("SelectedStop", typeof(GradientStop), typeof(GradientSlider));

		public GradientStop SelectedStop {
			get { return (GradientStop)GetValue(SelectedStopProperty); }
			set { SetValue(SelectedStopProperty, value); }
		}

		public static readonly DependencyProperty GradientStopsProperty =
			DependencyProperty.Register("GradientStops", typeof(BindingList<GradientStop>), typeof(GradientSlider));

		public BindingList<GradientStop> GradientStops {
			get { return (BindingList<GradientStop>)GetValue(GradientStopsProperty); }
			set { SetValue(GradientStopsProperty, value); }
		}

		public static Color GetColorAtOffset(IList<GradientStop> stops, double offset)
		{
			GradientStop s1 = stops[0], s2 = stops.Last();
			foreach (var item in stops) {
				if (item.Offset < offset && item.Offset > s1.Offset) s1 = item;
				if (item.Offset > offset && item.Offset < s2.Offset) s2 = item;
			}
			return Color.FromArgb(
				(byte)((s1.Color.A + s2.Color.A) / 2),
				(byte)((s1.Color.R + s2.Color.R) / 2),
				(byte)((s1.Color.G + s2.Color.G) / 2),
				(byte)((s1.Color.B + s2.Color.B) / 2)
			);
		}

		static void ThumbDragDelta(object sender, DragDeltaEventArgs e)
		{
			(sender as GradientSlider).thumb_DragDelta(sender, e);
		}

        static void ThumbDragStarted(object sender, DragStartedEventArgs e) 
        {
            isThumbDragInProgress = true;
        }

        static void ThumbDragCompleted(object sender, DragCompletedEventArgs e) 
        {
            isThumbDragInProgress = false;
        }

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			
			if (e.Property == BrushProperty && !isThumbDragInProgress ) {
				if (Brush != null) {
					GradientStops = new BindingList<GradientStop>(Brush.GradientStops);
					if (SelectedStop == null)
						SelectedStop = GradientStops.FirstOrDefault();
				}
				else {
					GradientStops = null;
				}
			}
		}

		void strip_DragStarted(object sender, DragStartedEventArgs e)
		{
			startOffset = e.HorizontalOffset / strip.ActualWidth;
			newStop = new GradientStop(GetColorAtOffset(GradientStops, startOffset), startOffset);
			GradientStops.Add(newStop);
			SelectedStop = newStop;
			e.Handled = true;
		}

		void strip_DragDelta(object sender, DragDeltaEventArgs e)
		{
			MoveStop(newStop, startOffset, e);
			e.Handled = true;
		}

		void thumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			var stop = (e.OriginalSource as GradientThumb).GradientStop;
			MoveStop(stop, stop.Offset, e);
		}

		void MoveStop(GradientStop stop, double oldOffset, DragDeltaEventArgs e)
		{
			if (e.VerticalChange > 50 && GradientStops.Count > 2) {
				GradientStops.Remove(stop);
				SelectedStop = GradientStops.FirstOrDefault();
				return;
			}
			stop.Offset = (oldOffset + e.HorizontalChange / strip.ActualWidth).Coerce(0, 1);
		}
	}

	public class GradientItemsControl : Selector
	{
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new GradientThumb();
		}
	}

	public class GradientThumb : Thumb
	{
		public GradientStop GradientStop {
			get { return DataContext as GradientStop; }
		}

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);
			var itemsControl = ItemsControl.ItemsControlFromItemContainer(this) as GradientItemsControl;
			itemsControl.SelectedItem = GradientStop;
		}
	}

	public class Dragger : Thumb
	{
	}
}
