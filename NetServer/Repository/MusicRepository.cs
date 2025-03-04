using NetServer.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using DnsClient.Protocol;

namespace NetServer.Repositories
{
    public class MusicRepository : IMusicRepository
    {

        private readonly IMongoCollection<User> _users;

        public MusicRepository(IMongoClient client)
        {
            var database = client.GetDatabase("mdtwo");
            _users = database.GetCollection<User>("Users");
        }

        public async Task<Music?> GetByMusicIdAsync(string userId, string musicId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            
            return user?.Music?.Find(m => m.ApiId == musicId);
        }

        public async Task<Music?> GetByMusicNameAsync(string userId, string musicName)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            
            return user?.Music?.Find(m => m.Title == musicName);
        }

        public async Task<IEnumerable<Music>> GetAllMusicByUserAsync(string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            
            return user?.Music ?? new List<Music>();
        }

        public async Task AddMusicAsync(string userId, Music music)
        {
            if (userId == null) throw new InvalidOperationException("User not found.");

            if (music == null) throw new ArgumentNullException(nameof(music));

            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync(); 

            // check dupe
            bool exists = user.Music?.Any(m => m.ApiId == music.ApiId) ?? false;
            if (exists) throw new InvalidOperationException("Music already exists for user");

            user.Music ??= new List<Music>(); // user doesnt have list so init
            user.Music.Add(music);

            var update = Builders<User>.Update.Push(u => u.Music, music);
            await _users.UpdateOneAsync(u => u.Id == userId, update);
        }


        public async Task<bool> UpdateMusicRatingAsync(string userId, string musicId, float rating)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
            
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync(); 

            var musicToUpdate = user.Music?.Find(m => m.ApiId == musicId); 
            if (musicToUpdate == null) return false; // no music

            // update | filter is for mongo to pick which doc to update
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId), 
                Builders<User>.Filter.ElemMatch(u => u.Music, m => m.ApiId == musicId)
            );
            var update = Builders<User>.Update.Set("Music.$.Rating", rating);
            var result = await _users.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteMusicAsync(string userId, string musicId)
        {
            if (string.IsNullOrWhiteSpace(musicId))
                {
                    throw new ArgumentNullException(nameof(musicId), "Music ID is required.");
                }

                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new InvalidOperationException("User not found.");
                }

                var musicToDelete = user!.Music?.Find(m => m.ApiId == musicId);
                if (musicToDelete == null) return false;
                
                // match user and music
                var filter = Builders<User>.Filter.And(
                    Builders<User>.Filter.Eq(u => u.Id, userId),
                    Builders<User>.Filter.ElemMatch(u => u.Music, m => m.ApiId == musicId)
                );

                // delete
                var update = Builders<User>.Update.Pull(u => u.Music, musicToDelete);
                var result = await _users.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0; // true if music was deleted
        }

    }
}