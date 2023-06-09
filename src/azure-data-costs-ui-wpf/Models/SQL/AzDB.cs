﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.SQL
{
    public class AzDB : BaseModel
    {
        public AzServer ParentAzServer { get; set; }
        public string ServerName { get; set; }

        //public string DatabaseName { get; set; }
        private string databaseName;
        public string DatabaseName
        {
            get => databaseName;
            set => SetProperty(ref databaseName, value);
        }

        public string Edition { get; set; }
        public string ServiceObjective { get; set; }

        public string MonthlyCost { get; set; }
        public string ElasticPoolName { get; set; }
        public float StorageMB { get; set; }
        public float AvgCpuPc { get; set; }
        public float AvgDataIoPc { get; set; }
        public float AvgLogWritePc { get; set; }
        public float MaxWorkerPc { get; set; }
        public float MaxSessionPc { get; set; }

        public List<DBPrincipal> DBPrincipals { get; set; } = new List<DBPrincipal>();
        public List<FireWallRule> DBFireWallRules { get; set; } = new List<FireWallRule>();
        public List<DBSyncState> SyncStates { get; set; } = new List<DBSyncState>();
        public List<Session> Sessions { get; set; } = new List<Session>();


        public async Task Refresh()
        {
            ExceptionMessages.Clear();
            ConnectivityError = "";
            ParentAzServer.ConnectivityError = "";

            Task[] tasks = new Task[4];

            tasks[0] = Task.Run(async () =>
            {
                var result = await ProcessResult(await DataAccess.GetDbPrincipals(ConnString));
                ConnectivityError = result.ExceptionMessage ;
                if (result.Result != null) DBPrincipals = (List<DBPrincipal>)result.Result;
            });
            tasks[1] = Task.Run(async () =>
            {
                var result = await ProcessResult(await DataAccess.GetDbFireWallRules(ConnString));
                ConnectivityError = result.ExceptionMessage ;
                if (result.Result != null) DBFireWallRules = (List<FireWallRule>)result.Result;
            });
            tasks[2] = Task.Run(async () =>
            {
                var result = await ProcessResult(await DataAccess.GetDbReplication(ConnString));
                ConnectivityError = result.ExceptionMessage ;
                if (result.Result != null) SyncStates = (List<DBSyncState>)result.Result;
            });
            tasks[3] = Task.Run(async () =>
            {
                var result = await ProcessResult(await DataAccess.GetDbSessions(ConnString));
                ConnectivityError = result.ExceptionMessage;
                if (result.Result != null) Sessions = (List<Session>)result.Result;
            });            


            await Task.WhenAll(tasks);
        }

        public void SetParent(AzServer parentServer)
        {
            ParentAzServer = parentServer;
            ServerName = parentServer.Name;
            ConnString = parentServer.ConnString.Replace("master", DatabaseName);
        }
    }
}
