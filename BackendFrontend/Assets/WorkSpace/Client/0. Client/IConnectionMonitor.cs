using System;
using Unity.VisualScripting;

public interface IConnectionMonitor : IInitializable, IDisposable
{
    void StartMonitoring();
}
