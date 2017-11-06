using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSharpCode.XamlDesigner.Helper
{
	static class Extension
	{
		public static int ParseDefault(this string value, int defaultValue)
		{
			return value == null ? defaultValue : (int) float.Parse(value);
		}
	}
}
