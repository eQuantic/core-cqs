using eQuantic.Core.CQS.Abstractions.Commands;
using eQuantic.Core.CQS.Abstractions.Queries;
using eQuantic.Core.CQS.Abstractions.Handlers;

namespace eQuantic.Core.CQS.Tests.Fixtures;

// ============================================================
// TEST COMMANDS & QUERIES
// ============================================================

public record TestCommand(string Value) : ICommand;
public record TestCommandWithResult(string Value) : ICommand<string>;
public record TestQuery(int Id) : IQuery<TestResult>;
public record TestResult(int Id, string Name);

// ============================================================
// TEST HANDLERS
// ============================================================

public sealed class TestCommandHandler : ICommandHandler<TestCommand>
{
    public List<string> ExecutedCommands { get; } = new();

    public Task Execute(TestCommand command, CancellationToken ct)
    {
        ExecutedCommands.Add(command.Value);
        return Task.CompletedTask;
    }
}

public sealed class TestCommandWithResultHandler : ICommandHandler<TestCommandWithResult, string>
{
    public Task<string> Execute(TestCommandWithResult command, CancellationToken ct)
    {
        return Task.FromResult($"Processed: {command.Value}");
    }
}

public sealed class TestQueryHandler : IQueryHandler<TestQuery, TestResult>
{
    public Task<TestResult> Execute(TestQuery query, CancellationToken ct)
    {
        return Task.FromResult(new TestResult(query.Id, $"Item-{query.Id}"));
    }
}

// ============================================================
// EXCEPTION THROWING HANDLERS
// ============================================================

public sealed class FailingCommandHandler : ICommandHandler<TestCommand>
{
    public Task Execute(TestCommand command, CancellationToken ct)
    {
        throw new InvalidOperationException($"Command failed: {command.Value}");
    }
}
