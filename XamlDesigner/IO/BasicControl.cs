using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ICSharpCode.XamlDesigner.IO
{
	public enum BasicControlType
	{
		Button = 1,
		Label = 2,
		TextBox = 3
	}

	class BasicControl
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("type")]
		public BasicControlType Type { get; set; }

		[JsonProperty("x")]
		public int X { get; set; }

		[JsonProperty("y")]
		public int Y { get; set; }

		[JsonProperty("width")]
		public int Width { get; set; }

		[JsonProperty("height")]
		public int Height { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }
	}
}
