using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;

namespace EDennis.EFBase {

    /// <summary>
    /// Provides a means of resetting SQL Server sequences
    /// to valid values.
    /// </summary>
    public class SequenceResetter {

        /// <summary>
        /// Resets all sequence values in a database.  The
        /// method uses the INFORMATION_SCHEMA schema to
        /// identify all sequences.  These sequences are
        /// reset to 1 plus the maximum value of the 
        /// column that uses the sequence's next value as
        /// a default.  Note: This method may fail if
        /// the same sequence is used for multiple tables.
        /// </summary>
        /// <param name="context">Valid db context</param>
        public static void ResetAllSequences(DbContext context) {
            SqlExecutor.Execute(context, sql);
        }

        public static void ResetAllSequences(string connectionString) {
            SqlExecutor.Execute(connectionString, sql);
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
			on rtrim(replace(replace(replace(replace(c.COLUMN_DEFAULT,'[',''),']',''),')',''),'(','')) LIKE '%next value for%' + s.SEQUENCE_NAME + '%' 

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
