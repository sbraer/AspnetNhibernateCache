# ASPNET Core web api rest with Mysql, Nhibernate and Redis

Web application in dotnet core 3.1.
Data source: MySql
ORM: Nhibernate
CACHE: Nhibernate module with REDIS

To test in local. With Docker:

`docker run -d --rm --name=mysql --env="MYSQL_ROOT_PASSWORD=mypassword" -p 3306:3306 mysql:8.0`

`docker run --rm --name redis -d -p 6379:6379 redis:6.2.5`

To check REDIS content:

`docker exec -it redis redis-cli`

And:

`keys * `
