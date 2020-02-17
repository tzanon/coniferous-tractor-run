using UnityEngine;

namespace Directions
{
	enum Direction { NULL = -1, CENTER, UP, RIGHT, DOWN, LEFT }

	public struct MovementVector
	{
		// properties
		
		public static MovementVector Up { get { return new MovementVector(Direction.UP); } }
		public static MovementVector Down { get { return new MovementVector(Direction.DOWN); } }
		public static MovementVector Right { get { return new MovementVector(Direction.RIGHT); } }
		public static MovementVector Left { get { return new MovementVector(Direction.LEFT); } }
		public static MovementVector Center { get { return new MovementVector(Direction.CENTER); } }
		public static MovementVector Null { get { return new MovementVector(Direction.NULL); } }

		public Vector3 Value { get; private set; }

		private MovementVector(Direction direction)
		{
			switch (direction)
			{
				case Direction.UP:
					Value = Vector3.up;
					break;
				case Direction.DOWN:
					Value = Vector3.down;
					break;
				case Direction.RIGHT:
					Value = Vector3.right;
					break;
				case Direction.LEFT:
					Value = Vector3.left;
					break;
				case Direction.CENTER:
					Value = Vector3.zero;
					break;
				default:
					Value = new Vector3(0, 0, -1);
					break;
			}
		}

		public static bool operator ==(MovementVector a, MovementVector b)
		{
			return a.Value == b.Value;
		}

		public static bool operator !=(MovementVector a, MovementVector b)
		{
			return a.Value != b.Value;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
