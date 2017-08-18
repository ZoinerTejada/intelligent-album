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
    public class ImageTagRepository
    {
        private static string ConnectionString { get; set; }
        public ImageTagRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public static IDbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public IEnumerable<ImageTag> GetAll()
        {
            var _db = GetConnection();
            List<ImageTag> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<ImageTag>("select * from ImageTag").ToList();
                _db.Close();
            }
            return result;
        }

        public IEnumerable<string> GetUniqueTagList()
        {
            var _db = GetConnection();
            List<string> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<string>("select distinct Tag from ImageTag").ToList();
                _db.Close();
            }
            return result;
        }

        public IEnumerable<ImageTag> GetByImageId(Guid imageId)
        {
            var _db = GetConnection();
            List<ImageTag> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<ImageTag>("select * from ImageTag where ImageId = @ImageId", new { ImageId = imageId }).ToList();
                _db.Close();
            }
            return result;
        }

        public IEnumerable<ImageTag> GetByTag(string tag)
        {
            var _db = GetConnection();
            List<ImageTag> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<ImageTag>("select * from ImageTag where Tag = @Tag", new { Tag = tag }).ToList();
                _db.Close();
            }
            return result;
        }

        public void Add(ImageTag imageTag)
        {
            var _db = GetConnection();
            using (_db)
            {
                _db.Open();
                const string sql = "insert into ImageTag(ImageId,Tag) values (@ImageId,@Tag)";
                _db.Execute(sql, imageTag);
                _db.Close();
            }
        }

        public void Update(ImageTag imageTag)
        {
            var _db = GetConnection();
            using (_db)
            {
                _db.Open();
                const string sql = "update ImageTag set Tag = @Tag where ImageId = @ImageId";
                _db.Execute(sql, imageTag);
                _db.Close();
            }
        }

        public void DeleteByImageId(Guid imageId)
        {
            var _db = GetConnection();
            using (_db)
            {
                _db.Open();
                const string sql = "delete from ImageTag where ImageId = @ImageId";
                _db.Execute(sql, new {ImageId = imageId});
                _db.Close();
            }
        }

    }
}
