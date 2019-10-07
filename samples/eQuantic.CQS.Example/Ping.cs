using eQuantic.Core.CQS.Commands;

namespace eQuantic.CQS.Example
{
    public class Ping : ICommand<Pong>
    {
        public string Message { get; set; }
    }
}