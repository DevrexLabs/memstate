using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Memstate.Configuration;
using Memstate.Host;
using Memstate.Models.Redis;
using Memstate.Runner.Commands;

namespace Memstate.Runner
{
    static class Extensions
    {
        public static bool TryMatch(this Regex regex, string input, out Match match)
        {
            match = regex.Match(input);
            return match.Success;
        }
    }

    public class SetCommand : Command<RedisModel>
    {
        public readonly string Key;
        public readonly string Value;

        public SetCommand(string key, string value)
        {
            Key = key;
            Value = value;
        }
        public override void Execute(RedisModel model)
        {
            model.Set(Key, Value);
        }
    }

    public class GetQuery : Query<RedisModel, String>
    {
        public readonly string Key;

        public GetQuery(string key)
        {
            Key = key;
        }
        public override string Execute(RedisModel db)
        {
            return db.Get(Key);
        }
    }

    class RedisServerCommand : ICommand
    {
        public event EventHandler Done;

        private Host<RedisModel> _host;
        private Task _cliTask;

        public async Task Start(string[] arguments)
        {
            var settings = Config.Current.GetSettings<EngineSettings>();
            settings.StreamName = "redis";
            _host = new HostBuilder<RedisModel>().UseWebConsole().Build();
            await _host.Start();
            _cliTask = Task.Run(RunCli);
        }

        public Task Stop() => _host.Stop();


        private async Task RunCli()
        {
            var set = new Regex(@"^\s*set (?<key>.+) (?<val>.+)$", RegexOptions.IgnoreCase);
            var get = new Regex(@"^\s*get (?<key>.+)$", RegexOptions.IgnoreCase);

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();

                if (set.TryMatch(line, out var setMatch))
                {
                    var key = setMatch.Groups["key"].Value;
                    var val = setMatch.Groups["val"].Value;
                    var command = new SetCommand(key, val);
                    await _host.TheEngine.Execute(command);
                }
                else if (get.TryMatch(line, out var getMatch))
                {
                    var key = getMatch.Groups["key"].Value;
                    var val = getMatch.Groups["val"].Value;
                    var query = new GetQuery(key);
                    string result = await _host.TheEngine.Execute(query);
                    Console.WriteLine(result);
                }
                else if (line == "exit") break;

                else Console.WriteLine("ERROR: Bad command");
            }
            Done?.Invoke(this, EventArgs.Empty);
        }
    }
}
