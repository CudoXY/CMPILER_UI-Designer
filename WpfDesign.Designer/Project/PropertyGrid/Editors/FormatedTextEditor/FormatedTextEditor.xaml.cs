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
using System.Windows.Documents;
using ICSharpCode.WpfDesign.UIExtensions;
using ICSharpCode.WpfDesign.Designer.themes;

namespace ICSharpCode.WpfDesign.Designer.PropertyGrid.Editors.FormatedTextEditor
{
	/// <summary>
	/// Interaktionslogik für FormatedTextEditor.xaml
	/// </summary>
	public partial class FormatedTextEditor
	{
		private DesignItem designItem;

		public FormatedTextEditor(DesignItem designItem)
		{
			SpecialInitializeComponent();

			this.designItem = designItem;

			var tb = ((TextBlock)designItem.Component);
			SetRichTextBoxTextFromTextBlock(richTextBox, tb);

			richTextBoxToolbar.RichTextBox = richTextBox;
			richTextBoxToolbar.SetValuesFromTextBlock(tb);

			richTextBox.Foreground = tb.Foreground;
			richTextBox.Background = tb.Background;
		}

		public static void SetRichTextBoxTextFromTextBlock(RichTextBox richTextBox, TextBlock textBlock)
		{
			IEnumerable<Inline> inlines = null;
			inlines = textBlock.Inlines.Select(x => CloneInline(x)).ToList();

			var paragraph = richTextBox.Document.Blocks.First() as Paragraph;
			paragraph.Inlines.AddRange(inlines);

			richTextBox.Document.Blocks.Add(paragraph);
		}

		/// <summary>
		/// Fixes InitializeComponent with multiple Versions of same Assembly loaded
		/// </summary>
		public void SpecialInitializeComponent()
		{
			if (!this._contentLoaded)
			{
				this._contentLoaded = true;
				Uri resourceLocator = new Uri(VersionedAssemblyResourceDictionary.GetXamlNameForType(this.GetType()), UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}

			this.InitializeComponent();
		}

		private static void GetDesignItems(DesignItem designItem, TextElementCollection<Block> blocks, List<DesignItem> list)
		{
			bool first = true;

			foreach (var block in blocks)
			{
				if (block is Paragraph)
				{
					if (!first)
					{
						list.Add(designItem.Services.Component.RegisterComponentForDesigner(new LineBreak()));
						list.Add(designItem.Services.Component.RegisterComponentForDesigner(new LineBreak()));
					}

					foreach (var inline in ((Paragraph)block).Inlines)
					{
						list.Add(InlineToDesignItem(designItem, inline));
					}
				}
				else if (block is Section)
				{
					GetDesignItems(designItem, ((Section)block).Blocks, list);
				}

				first = false;
			}
		}

		private static Inline CloneInline(Inline inline)
		{
			Inline retVal = null;
			if (inline is LineBreak)
				retVal = new LineBreak();
			else if (inline is Span)
				retVal = new Span();
			else if (inline is Run)
			{
				retVal = new Run(((Run)inline).Text);
			}

			if (inline.ReadLocalValue(Inline.BackgroundProperty) != DependencyProperty.UnsetValue)
				retVal.Background = inline.Background;
			if (inline.ReadLocalValue(Inline.ForegroundProperty) != DependencyProperty.UnsetValue)
				retVal.Foreground = inline.Foreground;
			if (inline.ReadLocalValue(Inline.FontFamilyProperty) != DependencyProperty.UnsetValue)
				retVal.FontFamily = inline.FontFamily;
			if (inline.ReadLocalValue(Inline.FontSizeProperty) != DependencyProperty.UnsetValue)
				retVal.FontSize = inline.FontSize;
			if (inline.ReadLocalValue(Inline.FontStretchProperty) != DependencyProperty.UnsetValue)
				retVal.FontStretch = inline.FontStretch;
			if (inline.ReadLocalValue(Inline.FontStyleProperty) != DependencyProperty.UnsetValue)
				retVal.FontStyle = inline.FontStyle;
			if (inline.ReadLocalValue(Inline.FontWeightProperty) != DependencyProperty.UnsetValue)
				retVal.FontWeight = inline.FontWeight;
			if (inline.ReadLocalValue(Inline.TextEffectsProperty) != DependencyProperty.UnsetValue)
				retVal.TextEffects = inline.TextEffects;
			if (inline.ReadLocalValue(Inline.TextDecorationsProperty) != DependencyProperty.UnsetValue)
				retVal.TextDecorations = inline.TextDecorations;

			return retVal;
		}

		private static DesignItem InlineToDesignItem(DesignItem designItem, Inline inline)
		{
			DesignItem d = d = designItem.Services.Component.RegisterComponentForDesigner(CloneInline(inline));
			if (inline is Run)
			{
				var run = inline as Run;

				if (run.ReadLocalValue(Run.TextProperty) != DependencyProperty.UnsetValue)
				{
					d.Properties.GetProperty(Run.TextProperty).SetValue(run.Text);
				}
			}
			else if (inline is Span)
			{ }
			else if (inline is LineBreak)
			{ }
			else
			{
				return null;
			}

			SetDesignItemTextpropertiesFromInline(d, inline);

			return d;
		}

		private static void SetDesignItemTextpropertiesFromInline(DesignItem targetDesignItem, Inline inline)
		{
			if (inline.ReadLocalValue(TextElement.BackgroundProperty) != DependencyProperty.UnsetValue)
				targetDesignItem.Properties.GetProperty(TextElement.BackgroundProperty).SetValue(inline.Background);
			if (inline.ReadLocalValue(TextElement.ForegroundProperty) != DependencyProperty.UnsetValue)
				targetDesignItem.Properties.GetProperty(TextElement.ForegroundProperty).SetValue(inline.Foreground);
			if (inline.ReadLocalValue(TextElement.FontFamilyProperty) != DependencyProperty.UnsetValue)
				targetDesignItem.Properties.GetProperty(TextElement.FontFamilyProperty).SetValue(inline.FontFamily);
			if (inline.ReadLocalValue(TextElement.FontSizeProperty) != DependencyProperty.UnsetValue)
				targetDesignItem.Properties.GetProperty(TextElement.FontSizeProperty).SetValue(inline.FontSize);
			if (inline.ReadLocalValue(TextElement.FontStretchProperty) != DependencyProperty.UnsetValue)
				targetDesignItem.Properties.GetProperty(TextElement.FontStretchProperty).SetValue(inline.FontStretch);
			if (inline.ReadLocalValue(TextElement.FontStyleProperty) != DependencyProperty.UnsetValue)
				targetDesignItem.Properties.GetProperty(TextElement.FontStyleProperty).SetValue(inline.FontStyle);
			if (inline.ReadLocalValue(TextElement.FontWeightProperty) != DependencyProperty.UnsetValue)
				targetDesignItem.Properties.GetProperty(TextElement.FontWeightProperty).SetValue(inline.FontWeight);
			if (inline.TextDecorations.Count > 0) {
				targetDesignItem.Properties.GetProperty("TextDecorations").SetValue(new TextDecorationCollection());
				var tdColl = targetDesignItem.Properties.GetProperty("TextDecorations");

				foreach (var td in inline.TextDecorations) {
					var newTd = targetDesignItem.Services.Component.RegisterComponentForDesigner(new TextDecoration());
					if (inline.ReadLocalValue(TextDecoration.LocationProperty) != DependencyProperty.UnsetValue)
						newTd.Properties.GetProperty(TextDecoration.LocationProperty).SetValue(td.Location);
					if (inline.ReadLocalValue(TextDecoration.PenProperty) != DependencyProperty.UnsetValue)
						newTd.Properties.GetProperty(TextDecoration.PenProperty).SetValue(td.Pen);

					tdColl.CollectionElements.Add(newTd);
				}
			}
		}

		public static void SetTextBlockTextFromRichTextBlox(DesignItem designItem, RichTextBox richTextBox)
		{
			designItem.Properties.GetProperty(TextBlock.TextProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.FontSizeProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.FontFamilyProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.FontStretchProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.FontWeightProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.BackgroundProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.ForegroundProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.FontStyleProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.TextEffectsProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.TextDecorationsProperty).Reset();

			var inlinesProperty = designItem.Properties.GetProperty("Inlines");
			inlinesProperty.CollectionElements.Clear();

			var doc = richTextBox.Document;
			//richTextBox.Document = new FlowDocument();

			var inlines = new List<DesignItem>();
			GetDesignItems(designItem, doc.Blocks, inlines);

			if (inlines.Count == 1 && inlines.First().Component is Run) {
				var run = inlines.First().Component as Run;
				SetDesignItemTextpropertiesFromInline(designItem, run);
				designItem.Properties.GetProperty(TextBlock.TextProperty).SetValue(run.Text);
			}
			else {
				foreach (var inline in inlines) {
					inlinesProperty.CollectionElements.Add(inline);
				}
			}
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			var changeGroup = designItem.OpenGroup("Formated Text");

			SetTextBlockTextFromRichTextBlox(designItem, richTextBox);

			changeGroup.Commit();

			this.TryFindParent<Window>().Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.TryFindParent<Window>().Close();
		}

		private void StrikeThroughButton_Click(object sender, RoutedEventArgs e)
		{
			TextRange range = new TextRange(richTextBox.Selection.Start, richTextBox.Selection.End);

			TextDecorationCollection tdc = (TextDecorationCollection)richTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);

			if (tdc == null || !tdc.Equals(TextDecorations.Strikethrough))
			{
				tdc = TextDecorations.Strikethrough;
			}
			else
			{
				tdc = null;
			}
			range.ApplyPropertyValue(Inline.TextDecorationsProperty, tdc);
		}
	}
}
