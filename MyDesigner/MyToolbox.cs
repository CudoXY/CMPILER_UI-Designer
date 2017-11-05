using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MyTestAssembly;

namespace MyDesigner
{
	public class MyToolbox
	{

		public MyToolbox()
		{
			MyControlNodes = new ObservableCollection<ControlNode>
			{
				new ControlNode {ControlType = ControlTypeEnum.Button},
				new ControlNode {ControlType = ControlTypeEnum.Label},
				new ControlNode {ControlType = ControlTypeEnum.TextBox}
			};

			//For poc, just add a node to our collection for each member of ControlTypeEnum.
		}

		public static MyToolbox Instance = new MyToolbox();
		public ObservableCollection<ControlNode> MyControlNodes { get; private set; }
	}

	//Represents either a widget or form controls node. A folder structure would require a different node for folders? Like the AssemblyNode in Toolbox.
	public class ControlNode
	{
		public ControlTypeEnum ControlType { get; set; }

		public string Name => this.ControlType.ToString();
	}
}
