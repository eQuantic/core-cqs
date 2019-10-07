using System.Threading.Tasks;
using eQuantic.Core.CQS;

namespace eQuantic.CQS.Example
{
    public static class Runner
    {
        public static async Task Run(IMediator mediator, WrappingWriter writer, string projectName)
        {
            var pong = await mediator.ExecuteAsync(new Ping { Message = "Ping" });
            await writer.WriteLineAsync("Executed: " + pong.Message);
        }
    }
}