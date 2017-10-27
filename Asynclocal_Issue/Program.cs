using System;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Injector;

namespace asynclocal
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceContainer();

            services.AddType<IFakeContextAccessor, FakeContextAccessor>(Lifetime.Singleton);
            services.AddType<IFakeContextFactory, FakeContextFactory>(Lifetime.Singleton);

            var resolver = services.Build();

            var factory = resolver.Resolve<IFakeContextFactory>();

            var currentContext = factory.Create();

            var accessor = resolver.Resolve<IFakeContextAccessor>();

            var context = accessor.FakeContext;

            //currentContext == context应为true
            Console.WriteLine(currentContext == context);

            //context != null应为true
            Console.WriteLine(context != null);

            Console.ReadKey();
        }
    }

    public class FakeContext { }

    public interface IFakeContextFactory
    {
        FakeContext Create();
    }

    [Intercept]
    public interface IFakeContextAccessor
    {
        FakeContext FakeContext { get; set; }
    }

    public class FakeContextFactory : IFakeContextFactory
    {
        private readonly IFakeContextAccessor _fakeContextAccessor;

        public FakeContextFactory(IFakeContextAccessor fakeContextAccessor)
        {
            _fakeContextAccessor = fakeContextAccessor;
        }
        public FakeContext Create()
        {
            var context = new FakeContext();
            if (_fakeContextAccessor != null)
            {
                _fakeContextAccessor.FakeContext = context;
            }
            return context;
        }
    }

    public class FakeContextAccessor : IFakeContextAccessor
    {
        private static readonly AsyncLocal<FakeContext> _currentContext = new AsyncLocal<FakeContext>();

        public FakeContext FakeContext
        {
            get
            {
                return _currentContext.Value;
            }
            set
            {
                _currentContext.Value = value;
            }
        }
    }

    public class Intercept : AspectCore.DynamicProxy.AbstractInterceptorAttribute
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            await next(context);
        }
    }
}
