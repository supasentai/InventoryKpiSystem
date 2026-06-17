using System.Threading.Channels;

namespace Inventory.Infrastructure.FileParsing;

public class FileProcessingQueue
{
    private readonly Channel<string> _queue;

    public FileProcessingQueue(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        _queue = Channel.CreateBounded<string>(options);
    }

    public async ValueTask EnqueueFileAsync(string filePath)
    {
        await _queue.Writer.WriteAsync(filePath);
    }

    public ChannelReader<string> Reader => _queue.Reader;
}
