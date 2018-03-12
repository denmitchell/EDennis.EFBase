using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using EDennis.MigrationsExtensions;

namespace EFBase.ConsoleTests.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateMaintenanceProcedures();
            migrationBuilder.CreateSequence<int>(
                name: "seqPerson");

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    PersonId = table.Column<int>(nullable: false, defaultValueSql: "next value for seqPerson"),
                    FirstName = table.Column<string>(maxLength: 20, nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    SysStart = table.Column<DateTime>(nullable: false, defaultValueSql: "(getdate())"),
                    SysEnd = table.Column<DateTime>(nullable: false, defaultValueSql: "(CONVERT(datetime2, '9999-12-31 23:59:59.9999999'))"),
                    SysUserId = table.Column<int>(nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.PersonId);
                });

            /*
            migrationBuilder.CreateTable(
                name: "SqlJsonResult",
                columns: table => new
                {
                    Json = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlJsonResult", x => x.Json);
                });
            */
            migrationBuilder.DoInserts("MigrationsInserts\\20180312125832_Insert.sql");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Person");

            //migrationBuilder.DropTable(
            //    name: "SqlJsonResult");

            migrationBuilder.DropSequence(
                name: "seqPerson");

            migrationBuilder.DropMaintenanceProcedures();

        }
    }
}
