using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using ICSharpCode.WpfDesign.Designer;
using ICSharpCode.WpfDesign.Designer.Xaml;
using ICSharpCode.WpfDesign.Designer.OutlineView;
using System.Xml;
using ICSharpCode.WpfDesign;
using ICSharpCode.WpfDesign.Designer.Services;
using System.Diagnostics;
using ICSharpCode.WpfDesign.XamlDom;
using ICSharpCode.XamlDesigner.IO;
using MyTestAssembly;

namespace ICSharpCode.XamlDesigner
{
	public class Document : INotifyPropertyChanged
	{
		public Document(string tempName, string text)
		{
			this.tempName = tempName;
			Text = text;
			IsDirty = false;
		}

		public Document(string filePath)
		{
			this.filePath = filePath;
			ReloadFile();
		}
		
		string tempName;
		DesignSurface designSurface = new DesignSurface();

		string text;

		public string Text {
			get {
				return text;
			}
			set {
				if (text != value) {
					text = value;
					IsDirty = true;
					RaisePropertyChanged("Text");
				}
			}
		}
		

		string filePath;

		public string FilePath {
			get {
				return filePath;
			}
			private set {
				filePath = value;
				RaisePropertyChanged("FilePath");
				RaisePropertyChanged("FileName");
				RaisePropertyChanged("Title");
				RaisePropertyChanged("Name");
			}
		}

		bool isDirty;

		public bool IsDirty {
			get {
				return isDirty;
			}
			private set {
				isDirty = value;
				RaisePropertyChanged("IsDirty");
				RaisePropertyChanged("Name");
				RaisePropertyChanged("Title");
			}
		}

		public XamlElementLineInfo xamlElementLineInfo;
		public XamlElementLineInfo XamlElementLineInfo
		{
			get
			{
				return xamlElementLineInfo;
			}
			private set
			{
				xamlElementLineInfo = value;
				RaisePropertyChanged("XamlElementLineInfo");
			}
		}

		public string FileName {
			get {
				if (FilePath == null) return null;
				return Path.GetFileName(FilePath);
			}
		}

		public string Name {
			get {
				return FileName ?? tempName;
			}
		}

		public string Title {
			get {
				return IsDirty ? Name + "*" : Name;
			}
		}

		public DesignSurface DesignSurface {
			get { return designSurface; }
		}

		public DesignContext DesignContext {
			get { return designSurface.DesignContext; }
		}

		public UndoService UndoService {
			get { return DesignContext.Services.GetService<UndoService>(); }
		}

		public ISelectionService SelectionService {
			get {
					return DesignContext.Services.Selection;
				}
		}

		public XamlErrorService XamlErrorService {
			get {
				if (DesignContext != null) {
					return DesignContext.Services.GetService<XamlErrorService>();
				}
				return null;
			}
		}

		IOutlineNode outlineRoot;

		public IOutlineNode OutlineRoot {
			get {
				return outlineRoot;
			}
			private set {
				outlineRoot = value;
				RaisePropertyChanged("OutlineRoot");
			}
		}

		void ReloadFile()
		{
			Text = File.ReadAllText(FilePath);
			UpdateDesign();
			IsDirty = false;
		}

		public void Save()
		{
			UpdateXaml();
			XamlToJsonWriter.Instance.Write(Text, FilePath);
			IsDirty = false;
		}

		public void SaveAs(string filePath)
		{
			FilePath = filePath;
			Save();
		}

		public void Refresh()
		{
			UpdateXaml();
			UpdateDesign();
		}

		void UpdateXaml()
		{
			var sb = new StringBuilder();
			using (var xmlWriter = new XamlXmlWriter(sb)) {
				DesignSurface.SaveDesigner(xmlWriter);
				Dictionary<XamlElementLineInfo, XamlElementLineInfo> d;
				Text = XamlFormatter.Format(sb.ToString(), out d);

				if (DesignSurface.DesignContext.Services.Selection.PrimarySelection != null)
				{
					var item = DesignSurface.DesignContext.Services.Selection.PrimarySelection;
					var line = ((PositionXmlElement) ((XamlDesignItem) item).XamlObject.XmlElement).LineNumber;
					var pos = (((XamlDesignItem)item).XamlObject.PositionXmlElement).LinePosition;
					var newP = d.FirstOrDefault(x => x.Key.LineNumber == line && x.Key.LinePosition == pos);
					XamlElementLineInfo = newP.Value;
				}
			}
		}

		public void UpdateDesign()
		{
			OutlineRoot = null;
			using (var xmlReader = XmlReader.Create(new StringReader(Text))) {
				XamlLoadSettings settings = new XamlLoadSettings();
				foreach (var assNode in Toolbox.Instance.AssemblyNodes)
				{
					settings.DesignerAssemblies.Add(assNode.Assembly);
				}
				settings.TypeFinder = MyTypeFinder.Instance;
				
				DesignSurface.LoadDesigner(xmlReader, settings);

				// Edit: Customize shown property
				DesignSurface.DesignContext.Services.AddOrReplaceService(typeof(IComponentPropertyService), new MyComponentPropertyService());
			}
			if (DesignContext.RootItem != null) {
				OutlineRoot = OutlineNode.Create(DesignContext.RootItem);
				UndoService.UndoStackChanged += new EventHandler(UndoService_UndoStackChanged);
			}
			RaisePropertyChanged("SelectionService");
			RaisePropertyChanged("XamlErrorService");
		}

		void UndoService_UndoStackChanged(object sender, EventArgs e)
		{
			IsDirty = true;
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChanged(string name)
		{
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion
	}
}
