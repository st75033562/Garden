namespace DataAccess
{
    public interface IDataSource
    {
        string Get(string tableName);
    }
}
