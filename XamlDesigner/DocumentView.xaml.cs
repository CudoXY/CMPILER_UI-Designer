using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

using ICSharpCode.WpfDesign.Designer.Services;

namespace ICSharpCode.XamlDesigner
{
	public partial class DocumentView
	{
		public DocumentView(Document doc)
		{
			InitializeComponent();

			Document = doc;
			Shell.Instance.Views[doc] = this;

			//uxTextEditor.DataBindings.Add("Text", doc, "Text", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged);

			Document.UpdateDesign();
			Document.RaisePropertyChanged("Mode");
			Document.RaisePropertyChanged("InDesignMode");
			Document.PropertyChanged += new PropertyChangedEventHandler(Document_PropertyChanged);
			uxTextEditor.TextChanged += new EventHandler(uxTextEditor_TextChanged);
		}

		void uxTextEditor_TextChanged(object sender, EventArgs e)
		{
			Document.Text = uxTextEditor.Text;
		}

		async void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Text" && Document.Text != uxTextEditor.Text)
				uxTextEditor.Text = Document.Text;
			if (e.PropertyName == "XamlElementLineInfo")
			{
				try
				{
					await Task.Delay(70);
					if (Document.XamlElementLineInfo != null)
					{
						uxTextEditor.SelectionStart = Document.XamlElementLineInfo.Position;
						uxTextEditor.SelectionLength = Document.XamlElementLineInfo.Length;
					}
					else
					{
						uxTextEditor.SelectionStart = 0;
						uxTextEditor.SelectionLength = 0;
					}

					uxTextEditor.Focus();
				}
				catch(Exception)
				{ }
			}
				
		}

		public Document Document { get; private set; }

	}
}
