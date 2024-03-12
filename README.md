## Simple-CRM-Fullstack-WebApp

This is a simple CRM (Customer Relationship Management) Fullstack WebApp, which is built using React and .Net.

#### Some Notes

The backend parts is built using .Net Core 8, and it connect with the database hosting on the docker. the connection string is in the appsettings.json file.

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=crm_database;Trusted_Connection=false;TrustServerCertificate=True;User Id=sa;Password=SQLConnect1;"}
```

`dotnet ef migrations add init`

`dotnet ef database update`

<!-- docker run -e "ACCEPT_EULA=1" -e "MSSQL_USER=SA" -e "MSSQL_SA_PASSWORD=SQLConnect1" -e "MSSQL_PID=Developer" -p 1433:1433 -d --name=sql_connect mcr.microsoft.com/azure-sql-edge -->
