using System;
using System.Collections.Generic;

public class Unsubscriber<CollectibleStatus> : IDisposable
{
	private List<IObserver<CollectibleStatus>> _totalObservers;
	private IObserver<CollectibleStatus> _observer;

	public Unsubscriber(List<IObserver<CollectibleStatus>> totalObservers, IObserver<CollectibleStatus> observer)
	{
		_totalObservers = totalObservers;
		_observer = observer;
	}

	public void Dispose()
	{
		if (_totalObservers.Contains(_observer))
			_totalObservers.Remove(_observer);
	}
}
