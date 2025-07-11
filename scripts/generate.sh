cd backend
dotnet tool restore
dotnet ef migrations script -o ../create.sql

# select 'drop table if exists "' || tablename || '" cascade;' from pg_tables;
# then remove pg_ and sql_ tables and trim quotes