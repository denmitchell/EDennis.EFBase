using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;

namespace EDennis.EFBase {
    public class SequenceResetter {

        public static void ResetAllSequences(DbContext context) {
            SqlExecutor.Execute(context, sql);
        }

        private static string sql =
@"
	declare @SequenceName varchar(255), @TableName nvarchar(255), @ColumnName nvarchar(255)
	declare @NextValueSql nvarchar(max), @SqlParamDef nvarchar(max), @NextValue int

	set @SqlParamDef = N'@NextValue int OUTPUT'

	declare @ResetSql nvarchar(max)

	declare crsr cursor for
	select 
		s.sequence_name SequenceName, c.table_name TableName, c.column_name ColumnName,
		'select @NextValue = max([' + c.column_name + ']) + 1 from [' + c.table_name + ']' NextValueSql
		from INFORMATION_SCHEMA.COLUMNS c
		inner join INFORMATION_SCHEMA.SEQUENCES s
			on c.COLUMN_DEFAULT LIKE '%' + s.SEQUENCE_NAME + '%' 

	open crsr
	while (1=1)
	begin
		fetch next from crsr into @SequenceName, @TableName, @ColumnName, @NextValueSql
		if @@FETCH_STATUS <> 0 
			break

		exec sp_executesql	@NextValueSql, 
							@SqlParamDef,
							@NextValue = @NextValue OUTPUT;

		set @ResetSql = 'alter sequence [' + @SequenceName + '] restart with ' + convert(varchar,ISNULL(@NextValue,1))
	
		exec sp_executesql @ResetSql;
	
	end

	close crsr
	deallocate crsr

";
    }
}
