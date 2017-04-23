using System.Collections.Generic;

namespace Verse.Schemas.Protobuf.Legacy
{
	public class VisitingNode
	{
		public Dictionary<int, VisitingNode> Children;

		public VisitingNode Parent;

		public int VisitCount;

		public VisitingNode(VisitingNode parent)
		{
			this.VisitCount = 1;
			this.Children = new Dictionary<int, VisitingNode>();
			this.Parent = parent;
		}
	}
}
