using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ICSharpCode.XamlDesigner.Helper;
using Newtonsoft.Json;

namespace ICSharpCode.XamlDesigner.IO
{
	class XamlToJsonWriter
	{
		private static string[] _controlList;
		public XamlToJsonWriter()
		{
			_controlList = Enum.GetNames(typeof(BasicControlType));
		}

		public static XamlToJsonWriter Instance = new XamlToJsonWriter();

		public void Write(string xaml, string path)
		{
			var xmlReader = XmlReader.Create(new StringReader(xaml));


			var form = new BasicForm();

			while (xmlReader.Read()) {

				// Read title
				if (xmlReader.Name.Equals("Window")) {
					var title = xmlReader.GetAttribute("Title");

					if (title != null) {
						form.Title = title;
						continue;
					}
				}


				var control = Array.Find(_controlList, c => c.Equals(xmlReader.Name));
				if (control == null || xmlReader.NodeType != XmlNodeType.Element)
					continue;

				Enum.TryParse(control, out BasicControlType controlType);

				var basicControl = new BasicControl
				{
					Type = controlType,
					Name = xmlReader.GetAttribute("Name"),
					X = xmlReader.GetAttribute("Canvas.Left").ParseDefault(0),
					Y = xmlReader.GetAttribute("Canvas.Top").ParseDefault(0),
					Width = xmlReader.GetAttribute("Width").ParseDefault(0),
					Height = xmlReader.GetAttribute("Height").ParseDefault(0),
					Text = controlType == BasicControlType.TextBox ? xmlReader.GetAttribute("Text") : xmlReader.GetAttribute("Content")
				};

				form.Controls.Add(basicControl);
			}

			if (form == null)
				throw new ArgumentNullException();

			using (var writer = new StreamWriter(path))
			{
				writer.WriteLine(JsonConvert.SerializeObject(form,
					Newtonsoft.Json.Formatting.None,
					new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore
					}));
			}
		}
	}
}
