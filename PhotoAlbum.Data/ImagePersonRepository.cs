using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using PhotoAlbum.DTO;

namespace PhotoAlbum.Data
{
    public class ImagePersonRepository
    {
        private static string ConnectionString { get; set; }
        public ImagePersonRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public static IDbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public IEnumerable<ImagePerson> GetAll()
        {
            var _db = GetConnection();
            List<ImagePerson> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<ImagePerson>("select * from ImagePerson").ToList();
                _db.Close();
            }
            return result;
        }

        public ImagePerson GetByImageAndPersonId(Guid imageId, Guid personId)
        {
            var _db = GetConnection();
            ImagePerson result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<ImagePerson>("select * from ImagePerson where ImageId = @ImageId and PersonId = @PersonId", new { ImageId = imageId, PersonId = personId }).FirstOrDefault();
                _db.Close();
            }
            return result;
        }

        public IEnumerable<ImagePerson> GetByImageId(Guid imageId)
        {
            var _db = GetConnection();
            List<ImagePerson> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<ImagePerson>("select * from ImagePerson where ImageId = @ImageId", new { ImageId = imageId }).ToList();
                _db.Close();
            }
            return result;
        }

        public IEnumerable<ImagePerson> GetByName(string name)
        {
            var _db = GetConnection();
            List<ImagePerson> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<ImagePerson>("select * from ImagePerson where Name = @Name", new { Name = name }).ToList();
                _db.Close();
            }
            return result;
        }

        public IEnumerable<string> GetUniqueNameList()
        {
            var _db = GetConnection();
            List<string> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<string>("select distinct Name from ImagePerson").ToList();
                _db.Close();
            }
            return result;
        }

        public void Add(ImagePerson imagePerson)
        {
            var _db = GetConnection();
            using (_db)
            {
                _db.Open();
                const string sql = "insert into ImagePerson(ImageId,PersonId,Name) values (@ImageId,@PersonId,@Name)";
                _db.Execute(sql, imagePerson);
                _db.Close();
            }
        }

        public void Update(ImagePerson imagePerson)
        {
            var _db = GetConnection();
            using (_db)
            {
                _db.Open();
                const string sql =
                    "update ImagePerson set Name = @Name where ImageId = @ImageId and PersonId = @PersonId";
                _db.Execute(sql, imagePerson);
                _db.Close();
            }
        }

        public void Delete(Guid imageId, Guid personId)
        {
            var _db = GetConnection();
            using (_db)
            {
                _db.Open();
                const string sql = "delete from ImagePerson where ImageId = @ImageId and PersonId = @PersonId";
                _db.Execute(sql, new {ImageId = imageId, PersonId = personId});
                _db.Close();
            }
        }

        public void DeleteByImageId(Guid imageId)
        {
            var _db = GetConnection();
            using (_db)
            {
                _db.Open();
                const string sql = "delete from ImagePerson where ImageId = @ImageId";
                _db.Execute(sql, new {ImageId = imageId});
                _db.Close();
            }
        }

    }
}
