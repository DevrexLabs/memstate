using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Memstate.Configuration;
using Memstate.Tcp;

namespace Memstate.Host
{
    public class Host
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;

        private const string CalledViaReflection = "The method is called via reflection.";

        private readonly object _server;

        private readonly MethodInfo _startMethod;

        private readonly MethodInfo _stopMethod;

        public Host(params string[] arguments)
        {
            Settings = Config.Current.Resolve<MemstateSettings>();

            var modelType = Type.GetType(Settings.Model);

            _startMethod = GetType()
                .GetMethods(BindingFlags)
                .Where(m => m.Name == "Start" && m.IsGenericMethod && m.GetGenericArguments().Length == 1)
                .Select(m => m.MakeGenericMethod(modelType))
                .First();

            _stopMethod = GetType()
                .GetMethods(BindingFlags)
                .Where(m => m.Name == "Stop" && m.IsGenericMethod && m.GetGenericArguments().Length == 1)
                .Select(m => m.MakeGenericMethod(modelType))
                .First();

            var modelCreator = Settings.CreateModelCreator();

            var model = CreateModel(modelType, modelCreator);

            _server = CreateServer(Settings, model);
        }

        public MemstateSettings Settings { get; }

        public void Start()
        {
            _startMethod.Invoke(null, new[] {_server});
        }

        public void Stop()
        {
            _stopMethod.Invoke(null, new[] {_server});
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = CalledViaReflection)]
        private static void Start<T>(MemstateServer<T> server) where T : class
        {
            server.Start();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = CalledViaReflection)]
        private static void Stop<T>(MemstateServer<T> server) where T : class
        {
            server.Stop();
        }

        private static object CreateModel(Type modelType, IModelCreator modelCreator)
        {
            var method = typeof(Host).GetMethods(BindingFlags)
                .Where(m => m.Name == "CreateModel" && m.IsGenericMethod && m.GetGenericArguments().Length == 1)
                .Select(m => m.MakeGenericMethod(modelType))
                .First();

            var model = method.Invoke(null, new object[] {modelCreator});
            return model;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = CalledViaReflection)]
        private static T CreateModel<T>(IModelCreator modelCreator)
        {
            var model = modelCreator.Create<T>();
            return model;
        }

        private static object CreateServer(MemstateSettings settings, object model)
        {
            var method = typeof(Host).GetMethods(BindingFlags)
                .Where(m => m.Name == "CreateServer" && m.IsGenericMethod && m.GetGenericArguments().Length == 1)
                .Select(m => m.MakeGenericMethod(model.GetType()))
                .First();

            var server = method.Invoke(null, new[] {settings, model});
            return server;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = CalledViaReflection)]
        private static MemstateServer<T> CreateServer<T>(MemstateSettings settings, T model) where T : class
        {
            var engine = new EngineBuilder().Build(model).Result;
            return new MemstateServer<T>(settings, engine);
        }
    }
}