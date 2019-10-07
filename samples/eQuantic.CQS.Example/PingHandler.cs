using System.IO;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.CQS;

namespace eQuantic.CQS.Example {
    public class PingHandler : ICommandHandler<Ping, Pong> {
        private readonly TextWriter writer;
        public PingHandler (TextWriter writer) {
            this.writer = writer;

        }
        public async Task<Pong> Execute (Ping command, CancellationToken cancellationToken) {
            await writer.WriteLineAsync($"--- Handled Ping: {command.Message}");
            return new Pong { Message = command.Message + " Pong" };
        }
    }
}