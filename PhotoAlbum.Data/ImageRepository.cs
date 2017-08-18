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
    public class ImageRepository
    {
        private static string ConnectionString { get; set; }
        public ImageRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public static IDbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public IEnumerable<Image> GetAll(int? maxRows)
        {
            var _db = GetConnection();
            List<Image> result;
            var top = maxRows.HasValue ? $"top({maxRows.Value})" : "";
            using (_db)
            {
                _db.Open();
                result = _db.Query<Image>($"select {top} * from Image order by DateCreated desc").ToList();
                _db.Close();
            }
            return result;
        }

        public IEnumerable<Image> GetByPersonId(Guid personId)
        {
            var _db = GetConnection();
            List<Image> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<Image>(@"select * from Image i
                                    where Id in (select ImageId from ImagePerson
                                    where PersonId = @PersonId)",
                   new
                   {
                       PersonId = personId
                   }).ToList();
                _db.Close();
            }
            return result;
        }

        public IEnumerable<Image> GetByPersonName(string name)
        {
            var _db = GetConnection();
            List<Image> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<Image>(@"select * from Image i
                                    where Id in (select ImageId from ImagePerson
                                    where Name = @Name)",
                   new
                   {
                       Name = name
                   }).ToList();
                _db.Close();
            }
            return result;
        }

        public IEnumerable<Image> GetByTag(string tag)
        {
            var _db = GetConnection();
            List<Image> result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<Image>(@"select * from Image i
                                    where Id in (select ImageId from ImageTag
                                    where Tag = @Tag)",
                   new
                   {
                       Tag = tag
                   }).ToList();
                _db.Close();
            }
            return result;
        }

        public Image GetById(Guid id)
        {
            var _db = GetConnection();
            Image result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<Image>("select * from Image where Id = @Id", new {Id = id}).FirstOrDefault();
                _db.Close();
            }
            return result;
        }

        public Image GetByImageName(string imageName)
        {
            var _db = GetConnection();
            Image result;
            using (_db)
            {
                _db.Open();
                result = _db.Query<Image>(@"select * from Image i
                                    where ImageName = @ImageName",
                   new
                   {
                       ImageName = imageName
                   }).FirstOrDefault();
                _db.Close();
            }
            return result;
        }

        public void Add(Image image)
        {
            if (image.Id == Guid.Empty)
                image.Id = Guid.NewGuid();
            var _db = GetConnection();
            using (_db)
            {
                _db.Open();
                const string sql =
                    "insert into Image(Id,ImageName,Description) values (@Id,@ImageName,@Description)";
                _db.Execute(sql, image);
                _db.Close();
            }
        }

        public void Update(Image image)
        {
            var _db = GetConnection();
            using (_db)
            {
                _db.Open();
                const string sql = "update Image set ImageName = @ImageName, Description = @Description where Id = @Id";
                _db.Execute(sql, image);
                _db.Close();
            }
        }

        public void Delete(Guid id)
        {
            var _db = GetConnection();
            using (_db)
            {
                _db.Open();
                const string sql = "delete from Image where Id = @Id";
                _db.Execute(sql, new {Id = id});
                _db.Close();
            }
        }

    }
}
