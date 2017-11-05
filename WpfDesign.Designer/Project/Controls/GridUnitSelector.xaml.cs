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
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.WpfDesign.Designer.themes;

namespace ICSharpCode.WpfDesign.Designer.Controls
{
	/// <summary>
	/// Interaction logic for GridUnitSelector.xaml
	/// </summary>
	public partial class GridUnitSelector
	{
		GridRailAdorner rail;

		public GridUnitSelector(GridRailAdorner rail)
		{
			SpecialInitializeComponent();

			this.rail = rail;
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

		void FixedChecked(object sender, RoutedEventArgs e)
		{
			this.rail.SetGridLengthUnit(Unit);
		}

		void StarChecked(object sender, RoutedEventArgs e)
		{
			this.rail.SetGridLengthUnit(Unit);
		}

		void AutoChecked(object sender, RoutedEventArgs e)
		{
			this.rail.SetGridLengthUnit(Unit);
		}

		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register("Orientation", typeof(Orientation), typeof(GridUnitSelector),
			                            new FrameworkPropertyMetadata());

		public Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		public DesignItem SelectedItem { get; set; }

		public GridUnitType Unit
		{
			get
			{
				if (auto.IsChecked == true)
					return GridUnitType.Auto;
				if (star.IsChecked == true)
					return GridUnitType.Star;

				return GridUnitType.Pixel;
			}
			set
			{
				switch (value)
				{
					case GridUnitType.Auto:
						auto.IsChecked = true;
						break;
					case GridUnitType.Star:
						star.IsChecked = true;
						break;
					default:
						@fixed.IsChecked = true;
						break;
				}
			}

		}
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			this.Visibility = Visibility.Hidden;
		}
	}

}
