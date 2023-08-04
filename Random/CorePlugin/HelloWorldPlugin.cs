using System;
using RandomStrangers.Tasks;

namespace RandomStrangers
{
    public class HelloWorld : Plugin
    {
        public override string name { get { return "Saying hello"; } } // to unload, /punload hello
        public override string creator { get { return Server.SoftwareName + " team"; } }
        public override string RandomStrangers_Version { get { return Server.Version; } }
        public override void Load(bool startup)
        {
            Server.Background.QueueOnce(SayHello, null, TimeSpan.FromSeconds(10));
        }

        void SayHello(SchedulerTask task)
        {
#if DEV_BUILD_RS
            Command.Find("say").Use(Player.Random, Server.SoftwareName + " " + Server.InternalVersion + " online!");
#else
            Command.Find("say").Use(Player.Console, Server.SoftwareName + " " + Server.InternalVersion + " online!");
#endif
            Logger.Log(LogType.SystemActivity, "&fHello World!");
        }
        public override void Unload(bool shutdown)
        {
        }

        public override void Help(Player p)
        {
            p.Message("");
        }
    }
}