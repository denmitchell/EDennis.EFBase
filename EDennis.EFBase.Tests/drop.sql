use master;
if db_id('a5921a9c76f7b466bbceb7227b855fb63') is not null
begin
alter database a5921a9c76f7b466bbceb7227b855fb63 set single_user with rollback immediate;
    drop database a5921a9c76f7b466bbceb7227b855fb63;
end