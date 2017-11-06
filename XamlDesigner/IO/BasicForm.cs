using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ICSharpCode.XamlDesigner.IO
{
	class BasicForm
	{
		public BasicForm()
		{
			Controls = new List<BasicControl>();
		}

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("controls")]
		public List<BasicControl> Controls { get; set; }
	}
}
