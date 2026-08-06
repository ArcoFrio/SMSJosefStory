namespace GameCreator.Runtime.Common;

public interface ISignalReceiver
{
    void OnReceiveSignal(SignalArgs args);
}