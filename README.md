# EFBase Wiki
The EFBase package provides a SqlRepo for Entity Framework targeting SQL Server 2016+.  The SqlRepo class includes methods for creating, reading (by PK), updating, and deleting -- both synchronously and asynchronously.  The following classes are included:
1. **[SqlRepo](https://github.com/denmitchell/efbase/wiki/SqlServerRepo)** -- which provides the base repository class.  
2. **[TestingTransaction](https://github.com/denmitchell/efbase/wiki/TestingTransaction)** -- which wraps a DbTransaction and provides the ability for a single transaction to be attached to multiple contexts and which can be configured to automatically roll back upon disposal.
3. **[SequenceResetter](https://github.com/denmitchell/efbase/wiki/SequenceResetter)**-- which provides a method for resetting all sequences in a database.
4. **[SqlExecutor](https://github.com/denmitchell/efbase/wiki/SqlExecutor)** -- which provides methods for executing multiple SQL statement blocks separated by GO.
5. **[UnitTestBase](https://github.com/denmitchell/efbase/wiki/UnitTestBase)** -- which provides a base class that allows Xunit tests that use SqlRepo and TestingTransaction to automatically roll back after each test case. 
6. **[IntegrationTestBase](https://github.com/denmitchell/efbase/wiki/IntegrationTestBase)** -- which which is like the UnitTestBase class, but which does not autorollback on Dispose(). 