using RabbitMQ.Client;

namespace mutation_seeker_orchestrator.src.scrapper.status;

public class WaitForQueue
{
    private readonly IModel _channel;
    private readonly object _lock = new ();
    private bool _canPublish = true;
    private readonly uint _maxMessages;
    

    public WaitForQueue(IModel channel, uint maxMessages)
    {
        _channel = channel;
        _maxMessages = maxMessages;
    }

    public async Task Wait()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                while (!_canPublish)
                {
                    Monitor.Wait(_lock);
                }
            }
        });
    }

    public void CheckAndLock()
    {
        lock (_lock)
        {
            _canPublish = CheckIfCanPublish();
            if (!_canPublish)
            {
                Monitor.Pulse(_lock);
            }
        }
    }

    public void CheckAndUnlock()
    {
        lock (_lock)
        {
            _canPublish = CheckIfCanPublish();
            if (_canPublish)
            {
                Monitor.Pulse(_lock);
            }
        }
    }

    private bool CheckIfCanPublish()
    {
        var messagesAmount = _channel.QueueDeclarePassive(_channel.CurrentQueue).MessageCount;
        return  messagesAmount < _maxMessages;
    }
}