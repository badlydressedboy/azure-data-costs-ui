using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.SQL
{
    public class AzServer : BaseModel
    {
        protected int _sqlConnTimeoutSecs = 3;

        public List<AzDB> ChildAzDBs { get; set; } = new List<AzDB>();
        public List<FireWallRule> FireWallRules { get; set; } = new List<FireWallRule>();
        public List<DBPrincipal> Logins { get; set; } = new List<DBPrincipal>();
        public List<EventLogEntry> EventLog { get; set; } = new List<EventLogEntry>();
        public List<EventLogEntry> MasterDbEventLog { get; set; } = new List<EventLogEntry>();
        


        public AzServer(string serverName)
        {
            Name = serverName;
            ConnString = $"Server={Name}.database.windows.net; Database=master; Connection Timeout={_sqlConnTimeoutSecs}; Application Name=AzureSqlMeta";
        }

        public async Task RefreshMetaData()
        {
            ExceptionMessages.Clear();
       
            Task[] tasks = new Task[2];

            tasks[0] = Task.Run(async () =>
            {
                var result = await ProcessResult(await DataAccess.GetServerFirewallRules(ConnString));
                ConnectivityError = result.ExceptionMessage ;
                if (result.Result != null)
                {
                    FireWallRules.Clear();
                    FireWallRules.AddRange((List<FireWallRule>)result.Result);
                }

            });
            tasks[1] = Task.Run(async () =>
            {
                var result = await ProcessResult(await DataAccess.GetServerEventLog(ConnString));
                ConnectivityError = result.ExceptionMessage ;
                if (result.Result != null)
                {
                    EventLog.Clear();
                    EventLog.AddRange((List<EventLogEntry>)result.Result);

                    MasterDbEventLog.Clear();
                    MasterDbEventLog.AddRange(EventLog.Where(x => x.Database == "master").ToList());

                    foreach (var db in ChildAzDBs) {

                        db.EventLog.Clear();
                        db.EventLog.AddRange(MasterDbEventLog.Where(x => x.Database == db.DatabaseName));
                    }
                }

            });


            await Task.WhenAll(tasks);
        }
    }
}
