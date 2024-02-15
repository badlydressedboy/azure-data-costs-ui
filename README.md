# azure-data-costs-ui
Drill down into individual Azure data related costs and see side by side expenditure on resources of given types

The UI is WPF so currently Windows only. I like the simplicity of not having to worry about a server and data access if governed by your login to azure, not a servers.

The layout for costs in the Azure portal is OK but I wanted a custom view so I can order lists of similar types (SQL Databases etc) and be able to drill down and see the properties appropriate for that resource type.

This could be extended to more resource types then just data but I care more about data so it's where I am starting.

Data access is via REST API and models are custom as Azure official helper classes had a bug at time of writing.

You need to be logged into azure either via CLI or Visual Studio in order to access REST API. No login or credential functionality is needed/offered in the app.


SQL Database Summary
![image](https://github.com/badlydressedboy/azure-data-costs-ui/assets/3395522/51c8a885-56bd-4641-b27c-f4cbd6ceb203)


SQL DB Cost Savings Analysis
![az-costs-analyze](https://github.com/badlydressedboy/azure-data-costs-ui/assets/3395522/2dcf6641-c031-4671-84b4-34d8b5e69f61)


SQL DB Live Status (sessions etc)
![az-db-status](https://github.com/badlydressedboy/azure-data-costs-ui/assets/3395522/60c345fd-398a-456d-bdd9-2586b580373d)


Storage costs drilled into:
![image](https://github.com/badlydressedboy/azure-data-costs-ui/assets/3395522/b8a51d8f-51c1-4e00-a518-7be9d1d2056c)


