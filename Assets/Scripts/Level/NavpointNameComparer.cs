using System.Collections.Generic;

public class NavpointNameComparer : IComparer<Navpoint>
{
	public int Compare(Navpoint x, Navpoint y) => string.Compare(x.name, y.name);
}
