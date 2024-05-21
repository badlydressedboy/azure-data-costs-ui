using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Azure.Costs.Common.Models.SQL;
using Azure.Core;
using Microsoft.Azure.Services.AppAuthentication;

namespace Azure.Costs.Common
{

    public static class DataAccess
    {
        private static string _sqlAccessToken;
        private static DateTime _sqlAccessTokenGetTime;
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // return object containing result data and nullable exception message
        private static async Task<string> GetSqlAccessToken()
        {
            if(_sqlAccessTokenGetTime > DateTime.Now.AddMinutes(-1))
            {
                return _sqlAccessToken;
            }
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            try
            {
                return await azureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net");
            }catch(Exception ex)
            {
                _logger.Error(ex);    
            }
            return null;
        }
        public static async Task<DataResult> GetServerDBs(string connString)
        {
            string sql = @"SELECT  d.name DatabaseName,   
                 slo.edition Edition,
	             slo.service_objective ServiceObjective,
	             slo.elastic_pool_name ElasticPoolName,
				 StorageMB,
				 AvgCpuPc,
				 AvgDataIoPc,
				 AvgLogWritePc,
				 MaxWorkerPc,
				 MazSessionPc
            FROM sys.databases d   
            JOIN sys.database_service_objectives slo    
            ON d.database_id = slo.database_id
			JOIN (
				SELECT  rs.database_name DBName, storage_in_megabytes StorageMB, avg_cpu_percent AvgCpuPc,
					avg_data_io_percent AvgDataIoPc, avg_log_write_percent AvgLogWritePc, max_worker_percent MaxWorkerPc, max_session_percent MazSessionPc
				FROM sys.resource_stats rs
					join (SELECT  database_name, max(start_time) maxstart
				FROM sys.resource_stats
				group by database_name)a on rs.database_name = a.database_name and rs.start_time = a.maxstart
			)r on d.name = r.dbname
            where d.database_id > 1;  
             ";
            return await GetQueryResult<AzDB>(sql, connString);
        }
        public static async Task<DataResult> GetServerFirewallRules(string connString)
        {
            string sql = @"select Name, start_ip_address StartIP, end_ip_address EndIP, create_date Created, modify_date Modified
                from sys.firewall_rules
                ;  
             ";
            return await GetQueryResult<FireWallRule>(sql, connString);
        }
        public static async Task<DataResult> GetServerEventLog(string connString)
        {
            string sql = @"select top 20 [database_name] [Database], start_time StartTime, end_time EndTime, 
	                event_category EventCategory, event_type EventType, event_subtype_desc EventSubType
	                , Severity, event_count EventCount, [Description]
                from [sys].event_log
                order by start_time desc
             ";
            return await GetQueryResult<EventLogEntry>(sql, connString);
        }

        private static async Task<DataResult> GetQueryResult<T>(string sql, string connString, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            var result = new DataResult();
            try
            {
               
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    conn.AccessToken = await GetSqlAccessToken();
                    conn.Open();

                    result.Result = await conn.QueryAsync<T>(sql);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.ExceptionMessage = $"{memberName}: {ex.Message}";
            }
            return result;
        }

        public static async Task<DataResult> GetDbPrincipals(string connString)
        {
            string sql = @"select name [Name], type_desc [Type], create_date [Created], modify_date [Modified]
                , authentication_type_desc [AuthType]
                from [sys].[database_principals]
                where tenant_id is not null
                order by name
                ;  
             ";
            return await GetQueryResult<DBPrincipal>(sql, connString);
        }

        public static async Task<DataResult> GetDbPermission(string connString)
        {
            string sql = @"SELECT permission_name FROM fn_my_permissions(NULL, 'DATABASE')
                        order by permission_name
                ;  
             ";
            return await GetQueryResult<string>(sql, connString);
        }

        public static async Task<DataResult> GetDbReplication(string connString)
        {
            string sql = @"select is_local IsLocal, is_primary_replica IsPrimaryReplica
	            , partner_server PartnerServer, partner_database PartnerDatabase
	            ,synchronization_state_desc SyncState,  synchronization_health_desc SyncHealth,
	            database_state_desc DBState, last_sent_time LastSentTime, log_send_rate LogSendRate
	            , secondary_lag_seconds SecondaryLagSeconds, role_desc Role, secondary_allow_connections_desc SecondaryAllowConnect
            from [sys].[dm_database_replica_states] rs
            left join [sys].[dm_geo_replication_link_status] ls
	            on rs.group_database_id = ls.link_guid
            order by is_local desc
                ;  
             ";
            return await GetQueryResult<DBSyncState>(sql, connString);
        }
        public static async Task<DataResult> GetDbSessions(string connString)
        {
            string sql = @"  select
     r.session_id SessionId,
     s.login_name LoginName,
     c.client_net_address IpAddress,
     s.host_name HostName,
     s.program_name ProgramName,
     st.text SqlText,
	 s.status Status,
    s.last_request_start_time LastRequestStartTime
 from sys.dm_exec_requests r
 inner join sys.dm_exec_sessions s
 on r.session_id = s.session_id
 left join sys.dm_exec_connections c
 on r.session_id = c.session_id
 outer apply sys.dm_exec_sql_text(r.sql_handle) st
 where  is_user_process = 1
 and program_name != 'TdService'
            ";
            return await GetQueryResult<Session>(sql, connString);
        }


        public static async Task<DataResult> GetDbFireWallRules(string connString)
        {
            string sql = @"select Name, start_ip_address StartIP, end_ip_address EndIP, create_date Created, modify_date Modified
            from sys.database_firewall_rules
;  
             ";
            return await GetQueryResult<FireWallRule>(sql, connString);
        }

        //public static async Task<List<DbProps>> GetDbReplicationStatus(string connString)
        //{
        //    var returnList = new List<DbProps>();

        //    var x = @"select is_local, is_primary_replica, partner_server, partner_database,synchronization_state_desc,  synchronization_health_desc,
        //             database_state_desc, last_sent_time, log_send_rate, secondary_lag_seconds,
        //              role_desc, secondary_allow_connections_desc
        //            from [sys].[dm_database_replica_states] rs
        //            left join [sys].[dm_geo_replication_link_status] ls
        //             on rs.group_database_id = ls.link_guid
        //            order by is_local desc
        //        ";

        //}
    }

    public class DataResult
    {
        public string? ExceptionMessage { get; set; }

        public Object? Result { get; set; }

        public DataResult() { }
    }
}
