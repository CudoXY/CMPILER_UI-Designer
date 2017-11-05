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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace ICSharpCode.WpfDesign.Designer.Controls
{
	public class NumericUpDown : Control
	{
		static NumericUpDown()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown),
				new FrameworkPropertyMetadata(typeof(NumericUpDown)));
		}
		
		TextBox textBox;
		DragRepeatButton upButton;
		DragRepeatButton downButton;

		public static readonly DependencyProperty DecimalPlacesProperty =
			DependencyProperty.Register("DecimalPlaces", typeof(int), typeof(NumericUpDown));

		public int DecimalPlaces {
			get { return (int)GetValue(DecimalPlacesProperty); }
			set { SetValue(DecimalPlacesProperty, value); }
		}

		public static readonly DependencyProperty MinimumProperty =
			DependencyProperty.Register("Minimum", typeof(double), typeof(NumericUpDown));

		public double Minimum {
			get { return (double)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}

		public static readonly DependencyProperty MaximumProperty =
			DependencyProperty.Register("Maximum", typeof(double), typeof(NumericUpDown),
			new FrameworkPropertyMetadata(100.0));

		public double Maximum {
			get { return (double)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(double?), typeof(NumericUpDown),
			new FrameworkPropertyMetadata(SharedInstances.BoxedDouble0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public double? Value {
			get { return (double?)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly DependencyProperty SmallChangeProperty =
			DependencyProperty.Register("SmallChange", typeof(double), typeof(NumericUpDown),
			new FrameworkPropertyMetadata(SharedInstances.BoxedDouble1));

		public double SmallChange {
			get { return (double)GetValue(SmallChangeProperty); }
			set { SetValue(SmallChangeProperty, value); }
		}

		public static readonly DependencyProperty LargeChangeProperty =
			DependencyProperty.Register("LargeChange", typeof(double), typeof(NumericUpDown),
			new FrameworkPropertyMetadata(10.0));

		public double LargeChange {
			get { return (double)GetValue(LargeChangeProperty); }
			set { SetValue(LargeChangeProperty, value); }
		}

		bool IsDragging {
			get {
				return upButton.IsDragging;
			}
			set {
				upButton.IsDragging = value; downButton.IsDragging = value;
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			upButton = (DragRepeatButton)Template.FindName("PART_UpButton", this);
			downButton = (DragRepeatButton)Template.FindName("PART_DownButton", this);
			textBox = (TextBox)Template.FindName("PART_TextBox", this);

			upButton.Click += upButton_Click;
			downButton.Click += downButton_Click;
			
			textBox.LostFocus += (sender, e) => OnLostFocus(e);

			var upDrag = new DragListener(upButton);
			var downDrag = new DragListener(downButton);

			upDrag.Started += drag_Started;
			upDrag.Changed += drag_Changed;
			upDrag.Completed += drag_Completed;

			downDrag.Started += drag_Started;
			downDrag.Changed += drag_Changed;
			downDrag.Completed += drag_Completed;

			Print();
		}

		void drag_Started(DragListener drag)
		{
			OnDragStarted();
		}

		void drag_Changed(DragListener drag)
		{
			IsDragging = true;
			MoveValue(-drag.DeltaDelta.Y * SmallChange);
		}

		void drag_Completed(DragListener drag)
		{
			IsDragging = false;
			OnDragCompleted();
		}

		void downButton_Click(object sender, RoutedEventArgs e)
		{
			if (!IsDragging) SmallDown();
		}

		void upButton_Click(object sender, RoutedEventArgs e)
		{
			if (!IsDragging) SmallUp();
		}

		protected virtual void OnDragStarted()
		{
		}

		protected virtual void OnDragCompleted()
		{
		}

		public void SmallUp()
		{
			MoveValue(SmallChange);
		}

		public void SmallDown()
		{
			MoveValue(-SmallChange);
		}

		public void LargeUp()
		{
			MoveValue(LargeChange);
		}

		public void LargeDown()
		{
			MoveValue(-LargeChange);
		}

		void MoveValue(double delta)
		{
			if (!Value.HasValue)
				return;

			double result;
			if (double.IsNaN((double)Value) || double.IsInfinity((double)Value)) {
				SetValue(delta);
			}
			else if (double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
				SetValue(result + delta);
			}
			else {
				SetValue((double)Value + delta);
			}
		}

		void Print()
		{
			if (textBox != null)
			{
				textBox.Text = Value?.ToString("F" + DecimalPlaces, CultureInfo.InvariantCulture);
				textBox.CaretIndex = int.MaxValue;
			}
		}

		//wpf bug?: Value = -1 updates bindings without coercing, workaround
		//update: not derived from RangeBase - no problem
		void SetValue(double? newValue)
		{
			newValue = CoerceValue(newValue);
			if (Value != newValue && !(Value.HasValue && double.IsNaN(Value.Value) && newValue.HasValue && double.IsNaN(newValue.Value)))
				Value = newValue;
		}

		double? CoerceValue(double? newValue)
		{
			if (!newValue.HasValue)
				return null;

			return Math.Max(Minimum, Math.Min((double) newValue, Maximum));
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);
			switch (e.Key) {
				case Key.Enter:
					SetInputValue();
					textBox.SelectAll();
					e.Handled = true;
					break;
				case Key.Up:
					SmallUp();
					e.Handled = true;
					break;
				case Key.Down:
					SmallDown();
					e.Handled = true;
					break;
				case Key.PageUp:
					LargeUp();
					e.Handled = true;
					break;
				case Key.PageDown:
					LargeDown();
					e.Handled = true;
					break;
//				case Key.Home:
//					Maximize();
//					e.Handled = true;
//					break;
//				case Key.End:
//					Minimize();
//					e.Handled = true;
//					break;
			}
		}

		void SetInputValue()
		{
			double result;
			if (double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
				SetValue(result);
			} else {
				Print();
			}
		}
		
		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			SetInputValue();
		}

		//protected override void OnMouseWheel(MouseWheelEventArgs e)
		//{
		//    if (e.Delta > 0)
		//    {
		//        if (Keyboard.IsKeyDown(Key.LeftShift))
		//        {
		//            LargeUp();
		//        }
		//        else
		//        {
		//            SmallUp();
		//        }
		//    }
		//    else
		//    {
		//        if (Keyboard.IsKeyDown(Key.LeftShift))
		//        {
		//            LargeDown();
		//        }
		//        else
		//        {
		//            SmallDown();
		//        }
		//    }
		//    e.Handled = true;
		//}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == ValueProperty) {
				Value = CoerceValue((double?)e.NewValue);
				Print();
			}
			else if (e.Property == SmallChangeProperty &&
			         ReadLocalValue(LargeChangeProperty) == DependencyProperty.UnsetValue) {
				LargeChange = SmallChange * 10;
			}
		}
	}

	public class DragRepeatButton : RepeatButton
	{
		public static readonly DependencyProperty IsDraggingProperty =
			DependencyProperty.Register("IsDragging", typeof(bool), typeof(DragRepeatButton));

		public bool IsDragging {
			get { return (bool)GetValue(IsDraggingProperty); }
			set { SetValue(IsDraggingProperty, value); }
		}
	}
}
