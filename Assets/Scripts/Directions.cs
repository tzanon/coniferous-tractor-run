using UnityEngine;

namespace Directions
{
	enum Direction { Null = -1, Center, Up, Right, Down, Left }

	public struct MovementVector
	{
		// properties
		
		public static MovementVector Up { get { return new MovementVector(Direction.Up); } }
		public static MovementVector Down { get { return new MovementVector(Direction.Down); } }
		public static MovementVector Right { get { return new MovementVector(Direction.Right); } }
		public static MovementVector Left { get { return new MovementVector(Direction.Left); } }
		public static MovementVector Center { get { return new MovementVector(Direction.Center); } }
		public static MovementVector Null { get { return new MovementVector(Direction.Null); } }

		public Vector3 Value { get; private set; }

		private MovementVector(Direction direction)
		{
			switch (direction)
			{
				case Direction.Up:
					Value = Vector3.up;
					break;
				case Direction.Down:
					Value = Vector3.down;
					break;
				case Direction.Right:
					Value = Vector3.right;
					break;
				case Direction.Left:
					Value = Vector3.left;
					break;
				case Direction.Center:
					Value = Vector3.zero;
					break;
				default:
					Value = new Vector3(0, 0, -1);
					break;
			}
		}

		public static MovementVector Vec3IntToMV(Vector3Int vec3I)
		{
			// invalid if a diagonal or on nonzero z plane
			if ((vec3I.x != 0 && vec3I.y != 0) || vec3I.z != 0)
			{
				return MovementVector.Null;
			}

			// right
			if (vec3I.x > 0)
			{
				return MovementVector.Right;
			}
			else if (vec3I.x < 0) // left
			{
				return MovementVector.Left;
			}
			if (vec3I.y > 0) // up
			{
				return MovementVector.Up;
			}
			else if (vec3I.y < 0) // down
			{
				return MovementVector.Down;
			}

			// must be (0,0)
			return MovementVector.Center;
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
