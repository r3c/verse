using System.IO;

using ProtoBuf;
using ProtoBuf.Meta;

namespace Verse.Schemas.Protobuf
{
	class ReaderState
	{
		#region Properties

		public int Position
		{
			get
			{
				return this.Reader.Position;
			}
		}

		public enum ReadingActionType
		{
			ReadHeader,

			UseHeader,

			ReadValue
		};

		public readonly DecodeError Error;        

		public VisitingNode ParentVisitingNode;

		public ProtoReader Reader;

		public ReadingActionType ReadingAction;

		public VisitingNode Root;

		private static readonly VisitingNode NoParent = null;

		#endregion

		#region Constructors

		public ReaderState(Stream stream, DecodeError error)
		{
			this.Error = error;
			this.Reader = new ProtoReader(stream, TypeModel.Create(), null);
			this.ReadingAction = ReadingActionType.ReadHeader;
			this.Root = new VisitingNode(NoParent);
			this.ParentVisitingNode = this.Root;
		}

		public bool ReadHeader(out int index)
		{
			index = this.Reader.ReadFieldHeader();

			return index > 0;
		}

		public void AddObject(int index)
		{
			VisitingNode obj;

			if (!this.ParentVisitingNode.Children.TryGetValue(index, out obj))
			{
				obj = new VisitingNode(this.ParentVisitingNode);

				this.ParentVisitingNode.Children.Add(index, obj);
			}
			else
			{
				++obj.VisitCount;
			}
		}

		public bool EnterObject(out int visitCount)
		{
			VisitingNode node;

			if (!this.ParentVisitingNode.Children.TryGetValue(this.Reader.FieldNumber, out node))
			{
				visitCount = 0;

				return false;
			}

			this.ParentVisitingNode = node;

			visitCount = node.VisitCount;

			return true;
		}

		public void LeaveObject()
		{
			this.ParentVisitingNode.Children.Clear();
			this.ParentVisitingNode = this.ParentVisitingNode.Parent;
		}

		#endregion
	}
}
