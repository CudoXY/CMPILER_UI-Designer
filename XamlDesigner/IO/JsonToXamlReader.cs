using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace ICSharpCode.XamlDesigner.IO
{
    class JsonToXamlReader
    {

	    public static JsonToXamlReader Instance = new JsonToXamlReader();

	    public string Read(string path)
	    {
			var json = File.ReadAllText(path);
			var results = JsonConvert.DeserializeObject<BasicForm>(json);

		    var root = new XElement("Window");
			root.Add(new XAttribute("placeholderproperty1", "placeholdervalue1"));
			root.Add(new XAttribute("placeholderproperty2", "placeholdervalue2"));
			root.Add(new XAttribute("Width", 640));
		    root.Add(new XAttribute("Height", 480));

			if (results.Title != null)
				root.Add(new XAttribute("Title", results.Title));

			var canvas = new XElement("Canvas");
			root.Add(canvas);

			for (var i = 0; i < results.Controls.Count; i++) {
			    var currItem = results.Controls[i];

				var control = new XElement(currItem.Type.ToString());
				canvas.Add(control);
			    control.Add(new XAttribute("Width", currItem.Width));
			    control.Add(new XAttribute("Height", currItem.Height));
			    control.Add(new XAttribute("Canvas.Left", currItem.X));
			    control.Add(new XAttribute("Canvas.Top", currItem.Y));
			    control.Add(new XAttribute(currItem.Type == BasicControlType.TextBox ? "Text" : "Content", currItem.Text + ""));
			}

		    var resultString = root.ToString().Replace("placeholderproperty1", "xmlns")
				.Replace("placeholdervalue1", "http://schemas.microsoft.com/winfx/2006/xaml/presentation")
			    .Replace("placeholderproperty2", "xmlns:x")
			    .Replace("placeholdervalue2", "http://schemas.microsoft.com/winfx/2006/xaml");
		    return resultString;
	    }
	}
}
