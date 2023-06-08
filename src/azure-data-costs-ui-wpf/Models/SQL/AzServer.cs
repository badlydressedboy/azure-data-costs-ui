using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.SQL
{
    public class AzServer : BaseModel
    {

        public List<AzDB> ChildAzDBs { get; set; } = new List<AzDB>();
        public List<FireWallRule> FireWallRules { get; set; } = new List<FireWallRule>();
        public List<DBPrincipal> Logins { get; set; } = new List<DBPrincipal>();
        public List<EventLogEntry> EventLog { get; set; } = new List<EventLogEntry>();


        public AzServer(string serverName)
        {
            Name = serverName;
            ConnString = $"Server={Name}.database.windows.net; Database=master; Connection Timeout=5; Application Name=AzureSqlMeta";
        }

        public async Task RefreshMetaData()
        {
            ExceptionMessages.Clear();
       
            Task[] tasks = new Task[2];

            tasks[0] = Task.Run(async () =>
            {
                var result = await ProcessResult(await DataAccess.GetServerFirewallRules(ConnString));
                ConnectivityError = result.ExceptionMessage ;
                if (result.Result != null) FireWallRules = (List<FireWallRule>)result.Result;

            });
            tasks[1] = Task.Run(async () =>
            {
                var result = await ProcessResult(await DataAccess.GetServerEventLog(ConnString));
                ConnectivityError = result.ExceptionMessage ;
                if (result.Result != null) EventLog = (List<EventLogEntry>)result.Result;

            });


            await Task.WhenAll(tasks);
        }
    }
}
