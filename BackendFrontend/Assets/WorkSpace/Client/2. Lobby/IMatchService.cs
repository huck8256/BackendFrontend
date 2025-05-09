using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public interface IMatchService : IInitializable, IDisposable
{
    event Action OnMatchRequested;
    event Action OnMatchCanceled;
    event Action OnMatchFound;
    event Action<List<string>> OnMatchSucceed;
    event Action OnMatchFailed;
    void RequestMatch();
    void RequestMatchCancel();
}
