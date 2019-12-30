using UnityEngine;

namespace Directions
{
	enum Direction { NULL = -1, CENTER, UP, RIGHT, DOWN, LEFT }

	public struct MovementVector
	{
		public static readonly MovementVector Up = new MovementVector(Direction.UP);
		public static readonly MovementVector Down = new MovementVector(Direction.DOWN);
		public static readonly MovementVector Right = new MovementVector(Direction.RIGHT);
		public static readonly MovementVector Left = new MovementVector(Direction.LEFT);
		public static readonly MovementVector Center = new MovementVector(Direction.CENTER);
		public static readonly MovementVector Null = new MovementVector(Direction.NULL);

		public readonly Vector3 _value;

		
		public Vector3 Value { get => _value; }

		private MovementVector(Direction direction)
		{
			switch (direction)
			{
				case Direction.UP:
					_value = Vector3.up;
					break;
				case Direction.DOWN:
					_value = Vector3.down;
					break;
				case Direction.RIGHT:
					_value = Vector3.right;
					break;
				case Direction.LEFT:
					_value = Vector3.left;
					break;
				case Direction.CENTER:
					_value = Vector3.zero;
					break;
				default:
					_value = new Vector3(0, 0, -1);
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
