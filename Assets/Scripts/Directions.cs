using UnityEngine;

namespace Directions
{
	enum DirectionNames { Null = -1, Center, North, East, South, West }

	public struct CardinalDirection
	{
		// properties
		public static CardinalDirection North { get { return new CardinalDirection(DirectionNames.North); } }
		public static CardinalDirection South { get { return new CardinalDirection(DirectionNames.South); } }
		public static CardinalDirection East { get { return new CardinalDirection(DirectionNames.East); } }
		public static CardinalDirection West { get { return new CardinalDirection(DirectionNames.West); } }
		public static CardinalDirection Center { get { return new CardinalDirection(DirectionNames.Center); } }
		public static CardinalDirection Null { get { return new CardinalDirection(DirectionNames.Null); } }

		public Vector3 Value { get; private set; }

		private CardinalDirection(DirectionNames direction)
		{
			switch (direction)
			{
				case DirectionNames.North:
					Value = Vector3.up;
					break;
				case DirectionNames.South:
					Value = Vector3.down;
					break;
				case DirectionNames.East:
					Value = Vector3.right;
					break;
				case DirectionNames.West:
					Value = Vector3.left;
					break;
				case DirectionNames.Center:
					Value = Vector3.zero;
					break;
				default:
					Value = new Vector3(0, 0, -1);
					break;
			}
		}

		public static CardinalDirection DirectionBetweenPoints(Vector3 from, Vector3 to)
		{
			var difference = to - from;
			CardinalDirection mv = CardinalDirection.Vec3ToCardinal(difference);
			return mv;
		}

		/// <summary>
		/// Get MovementVector representation of a Vector3
		/// </summary>
		/// <param name="vec3"></param>
		/// <returns>MovementVector rep of vec3</returns>
		public static CardinalDirection Vec3ToCardinal(Vector3 vec3)
		{
			if (vec3.x > 0) return CardinalDirection.East;
			if (vec3.x < 0) return CardinalDirection.West;
			if (vec3.y > 0) return CardinalDirection.North;
			if (vec3.y < 0) return CardinalDirection.South;

			return CardinalDirection.Center;
		}

		public static bool operator ==(CardinalDirection a, CardinalDirection b)
		{
			return a.Value == b.Value;
		}

		public static bool operator !=(CardinalDirection a, CardinalDirection b)
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
