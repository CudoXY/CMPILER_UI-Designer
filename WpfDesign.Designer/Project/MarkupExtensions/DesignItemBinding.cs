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
using System.Windows.Data;
using System.Windows.Markup;
using ICSharpCode.WpfDesign.UIExtensions;

[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "ICSharpCode.WpfDesign.Designer.MarkupExtensions")]


namespace ICSharpCode.WpfDesign.Designer.MarkupExtensions
{
	/// <summary>
	/// A Binding to a DesignItem of Object
	/// 
	/// This can be used for Example your own Property Pages for Designer Objects
	/// </summary>
	public class DesignItemBinding : MarkupExtension
	{
		private string _propertyName;
		private DependencyProperty _property;
		private Binding _binding;
		private DesignItemSetConverter _converter;
		private DependencyProperty _targetProperty;
		private FrameworkElement _targetObject;

		public bool SingleItemProperty { get; set; }
		
		public bool AskWhenMultipleItemsSelected { get; set; }
		
		public IValueConverter Converter { get; set; }
		
		public object ConverterParameter { get; set; }
		
		public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

		public UpdateSourceTrigger? UpdateSourceTriggerMultipleSelected { get; set; }

		public DesignItemBinding(string path)
		{
			this._propertyName = path;
			
			UpdateSourceTrigger = UpdateSourceTrigger.Default;
			AskWhenMultipleItemsSelected = true;
		}

		public DesignItemBinding(DependencyProperty property)
		{
			this._property = property;

			UpdateSourceTrigger = UpdateSourceTrigger.Default;
			AskWhenMultipleItemsSelected = true;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			IProvideValueTarget service = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
			_targetObject = service.TargetObject as FrameworkElement;
			_targetProperty = service.TargetProperty as DependencyProperty;

			if (_targetObject != null)
			{
				_targetObject.DataContextChanged += targetObject_DataContextChanged;
			}

			return null;
		}

		public void CreateBindingOnProperty(DependencyProperty targetProperty, FrameworkElement targetObject)
		{
			_targetProperty = targetProperty;
			_targetObject = targetObject;
			_targetObject.DataContextChanged += targetObject_DataContextChanged;
			targetObject_DataContextChanged(_targetObject, new DependencyPropertyChangedEventArgs());
		}
		
		void targetObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var dcontext = ((FrameworkElement) sender).DataContext;
			
			DesignContext context = null;
			FrameworkElement fe = null;
			DesignItem designItem = null;
			
			if (dcontext is DesignItem) {
				designItem = (DesignItem)dcontext;
				context = designItem.Context;
				fe = designItem.View as FrameworkElement;
			} else if (dcontext is FrameworkElement) {
				fe = ((FrameworkElement)dcontext);
				var srv = fe.TryFindParent<DesignSurface>();
				if (srv != null) {
					context = srv.DesignContext;
					designItem = context.Services.Component.GetDesignItem(fe);
				}
			}

			if (context != null)
			{
				if (_property != null)
				{
					_binding = new Binding();
					_binding.Path = new PropertyPath(_property);
					_binding.Source = fe;
					_binding.UpdateSourceTrigger = UpdateSourceTrigger;

					if (designItem.Services.Selection.SelectedItems.Count > 1 && UpdateSourceTriggerMultipleSelected != null)
					{
						_binding.UpdateSourceTrigger = UpdateSourceTriggerMultipleSelected.Value;
					}

					_binding.Mode = BindingMode.TwoWay;
					_binding.ConverterParameter = ConverterParameter;

					_converter = new DesignItemSetConverter(designItem, _property, SingleItemProperty, AskWhenMultipleItemsSelected,
						Converter);
					_binding.Converter = _converter;

					_targetObject.SetBinding(_targetProperty, _binding);
				}
				else
				{
					_binding = new Binding(_propertyName);
					_binding.Source = fe;
					_binding.UpdateSourceTrigger = UpdateSourceTrigger;

					if (designItem.Services.Selection.SelectedItems.Count > 1 && UpdateSourceTriggerMultipleSelected != null)
					{
						_binding.UpdateSourceTrigger = UpdateSourceTriggerMultipleSelected.Value;
					}

					_binding.Mode = BindingMode.TwoWay;
					_binding.ConverterParameter = ConverterParameter;

					_converter = new DesignItemSetConverter(designItem, _propertyName, SingleItemProperty, AskWhenMultipleItemsSelected,
						Converter);
					_binding.Converter = _converter;

					_targetObject.SetBinding(_targetProperty, _binding);
				}
			}
			else
			{
				_targetObject.ClearValue(_targetProperty);
			}
		}

		private class DesignItemSetConverter : IValueConverter
		{
			private DesignItem _designItem;
			private string _propertyName;
			private DependencyProperty _property;
			private bool _singleItemProperty;
			private bool _askWhenMultipleItemsSelected;
			private IValueConverter _converter;

			public DesignItemSetConverter(DesignItem desigItem, string propertyName, bool singleItemProperty, bool askWhenMultipleItemsSelected, IValueConverter converter)
			{
				this._designItem = desigItem;
				this._propertyName = propertyName;
				this._singleItemProperty = singleItemProperty;
				this._converter = converter;
				this._askWhenMultipleItemsSelected = askWhenMultipleItemsSelected;
			}

			public DesignItemSetConverter(DesignItem desigItem, DependencyProperty property, bool singleItemProperty, bool askWhenMultipleItemsSelected, IValueConverter converter)
			{
				this._designItem = desigItem;
				this._property = property;
				this._propertyName = property.Name;
				this._singleItemProperty = singleItemProperty;
				this._converter = converter;
				this._askWhenMultipleItemsSelected = askWhenMultipleItemsSelected;
			}

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (_converter != null)
					return _converter.Convert(value, targetType, parameter, culture);
				
				return value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				var val = value;
				if (_converter != null)
					val = _converter.ConvertBack(value, targetType, parameter, culture);
				
				var changeGroup = _designItem.OpenGroup("Property: " + _propertyName);

				try {
					DesignItemProperty property = null;

					if (_property != null) {
						try {
							property = _designItem.Properties.GetProperty(_property);
						}
						catch (Exception) {
							property = _designItem.Properties.GetAttachedProperty(_property);
						}
					}
					else {
						property = _designItem.Properties.GetProperty(_propertyName);
					}
					
					property.SetValue(val);

					if (!_singleItemProperty && _designItem.Services.Selection.SelectedItems.Count > 1)
					{
						var msg = MessageBoxResult.Yes;
						if (_askWhenMultipleItemsSelected) {
							msg = MessageBox.Show("Apply changes to all selected Items","", MessageBoxButton.YesNo);
						}
						if (msg == MessageBoxResult.Yes)
						{
							foreach (var item in _designItem.Services.Selection.SelectedItems)
							{
								try
								{
									if (_property != null)
										property = item.Properties.GetProperty(_property);
									else
										property = item.Properties.GetProperty(_propertyName);
								}
								catch(Exception)
								{ }
								if (property != null)
									property.SetValue(val);
							}
						}
					}

					changeGroup.Commit();
				}
				catch (Exception)
				{
					changeGroup.Abort();
				}

				return val;
			}
		}
	}
}
