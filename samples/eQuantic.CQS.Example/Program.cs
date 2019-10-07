using System;
using System.IO;
using System.Threading.Tasks;
using eQuantic.Core.CQS;
using eQuantic.Core.CQS.Pipeline;
using Lamar;

namespace eQuantic.CQS.Example
{
    class Program
    {
        static Task Main(string[] args)
        {
            var writer = new WrappingWriter(Console.Out);
            var mediator = BuildMediator(writer);

            return Runner.Run(mediator, writer, "Lamar");
        }

        private static IMediator BuildMediator(WrappingWriter writer)
        {
            var container = new Lamar.Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.AssemblyContainingType<Ping>();
                    scanner.ConnectImplementationsToTypesClosing(typeof(ICommandHandler<,>));
                    scanner.ConnectImplementationsToTypesClosing(typeof(IQueryHandler<,>));
                });

                //Pipeline
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(PreProcessorBehavior<,>));
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(PostProcessorBehavior<,>));

                // This is the default but let's be explicit. At most we should be container scoped.
                cfg.For<IMediator>().Use<Mediator>().Transient();

                cfg.For<eQuantic.Core.Ioc.IContainer>().Use(ctx => new Container(ctx));
                cfg.For<TextWriter>().Use(writer);
            });


            var mediator = container.GetInstance<IMediator>();

            return mediator;
        }
    }
}