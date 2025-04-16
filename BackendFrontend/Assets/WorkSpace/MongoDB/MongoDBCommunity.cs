using MongoDB.Bson;
using MongoDB.Driver;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MongoDBCommunity : SingletonMonoBehaviour<MongoDBCommunity>
{
    [SerializeField] int port = 27017;      // MongoDB Default Port
    [SerializeField] string dBName = "DB";
    [SerializeField] string collectionName = "Data";

    MongoClient client;
    IMongoCollection<BsonDocument> collection;
    private void Start()
    {
        string connectionString = $"mongodb://localhost:{port}";

        // MongoDB ����
        client = new MongoClient(connectionString);

        // DB ��������
        IMongoDatabase database = client.GetDatabase(dBName);

        // Collection ĳ��
        collection = database.GetCollection<BsonDocument>(collectionName);
    }
    public async UniTask<bool> TryInsertAccountDataAsync(string id, string password)
    {
        // �ߺ� �˻�
        if (await IsUnique("ID", id))
        {
            BsonDocument document = new BsonDocument
            {
                { "ID", id },
                { "Password", password }
            };

            // Data ����
            await collection.InsertOneAsync(document);

            Debug.Log($"[DataBase] account has been created: {document["_id"]}.");
            return true;
        }

        Debug.Log("[DataBase] Please use a different ID");
        return false;
    }
    public async UniTask UpdateDataAsync(string guid, string key, string value)
    {
        // Update Insert �ɼ�
        var options = new UpdateOptions { IsUpsert = true };

        var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(guid));
        var update = Builders<BsonDocument>.Update.Set(key, value);

        await collection.UpdateOneAsync(filter, update, options);
        Debug.Log("[DataBase] Data Update");
    }
    public async UniTask<BsonDocument> GetBsonDocumentAsync(string key, string value)
    {
        var filter = Builders<BsonDocument>.Filter.Eq(key, value);
        var result = await collection.Find(filter).FirstOrDefaultAsync();

        if(result != null)
            return result;

        Debug.Log("[DataBase] Data does not exist");
        return null;
    }
    public async UniTask<bool> IsUnique(string key, string value)
    {
        // Key Value �� ã��
        var filter = Builders<BsonDocument>.Filter.Eq(key, value);
        var result = await collection.Find(filter).FirstOrDefaultAsync();

        if (result != null)
        {
            Debug.Log($"[DataBase] {key} already exists.");
            return false;
        }

        return true;
    }
}
