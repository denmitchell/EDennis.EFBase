use master;
if db_id('a5921a9c76f7b466bbceb7227b855fb63') is not null
begin
alter database a5921a9c76f7b466bbceb7227b855fb63 set single_user with rollback immediate;
    drop database a5921a9c76f7b466bbceb7227b855fb63;
end
go
create database a5921a9c76f7b466bbceb7227b855fb63;
go
use a5921a9c76f7b466bbceb7227b855fb63;
create sequence seqPerson as int start with 1;
create table Person(
	PersonId int not null default next value for seqPerson,
	LastName varchar(30),
	FirstName varchar(30),
	SysStart datetime2(0) generated always as row start default getdate(),
	SysEnd datetime2(0) generated always as row end default CONVERT(datetime2 (0), '9999-12-31 23:59:59.999999'),
	period for system_time (SysStart, SysEnd),
	constraint pkPerson 
		primary key (PersonId)
) with (system_versioning = on (history_table = dbo.PersonHistory));
