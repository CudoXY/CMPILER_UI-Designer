﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.WpfDesign.Designer.OutlineView;
using ICSharpCode.XamlDesigner; //Only for the ExtensionMethods class.
using ICSharpCode.WpfDesign.Designer.Services;
using MyTestAssembly;

namespace MyDesigner
{
	/// <summary>
	/// Interaction logic for MyToolboxView.xaml
	/// </summary>
	public partial class MyToolboxView : UserControl
	{
		public MyToolboxView()
		{
			this.DataContext = MyToolbox.Instance;
			InitializeComponent();

			new DragListener(this).DragStarted += Toolbox_DragStarted;
			uxTreeView.SelectedItemChanged += uxTreeView_SelectedItemChanged;
			uxTreeView.GotKeyboardFocus += uxTreeView_GotKeyboardFocus;
		}

		void uxTreeView_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			PrepareTool(uxTreeView.SelectedItem as ControlNode, false);
		}

		void uxTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			PrepareTool(uxTreeView.SelectedItem as ControlNode, false);
		}

		void Toolbox_DragStarted(object sender, MouseButtonEventArgs e)
		{
			PrepareTool(e.GetDataContext() as ControlNode, true);
		}

		void PrepareTool(ControlNode node, bool drag)
		{
			if (node != null)
			{

				//Get the Type we want to use for the ControlNode being dragged out here.
				Type type = GetTypeForControlType(node.ControlType);

				var tool = new CreateComponentTool(type);
				if (MyDesignerModel.Instance.DesignSurface != null)
				{
					MyDesignerModel.Instance.DesignSurface.DesignContext.Services.Tool.CurrentTool = tool;
					if (drag)
					{
						DragDrop.DoDragDrop(this, tool, DragDropEffects.Copy);
					}
				}
			}
		}

		private Type GetTypeForControlType(ControlTypeEnum controlType)
		{
			switch (controlType) {
				case ControlTypeEnum.Button:
					return typeof(Button);
				case ControlTypeEnum.Label:
					return typeof(Label);
				case ControlTypeEnum.TextBox:
					return typeof(TextBox);
			}

			return null;
		}
	}
}
