using System;
using log4net;
using GameCollectionManager.Shared.Models;
using System.Data;
using System.Net.Security;
using Npgsql;
using Newtonsoft.Json;
using GameCollectionManagerAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace GameCollectionManagerAPI.Services
{
    public class DB_Services : IDB_Service
    {
        List<GameDAO> gameList = new List<GameDAO>();
        private ILog log = LogManager.GetLogger(typeof(Program));
        private readonly IConfiguration _configuration;
        private DataContext _context;

        public DB_Services(IConfiguration configuration, DataContext dataContext)
        {
            this._configuration = configuration;
            _context = dataContext;
        }

        public async Task<List<GameDAO>> GetGamesAsync(string user)
        {
            try
            {
                if (user == null)
                {
                    user = "NoUser";
                }
                return await _context.Games.Where(c => c.owner == user).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<GameDAO>();
            }
        }
        public async Task<List<GameDAO>> GetQueueAsync(string user)
        {
            try
            {
                if (user == null)
                {
                    user = "NoUser";
                }
                return await _context.Games.Where(c => c.owner == user && c.queuepos != null).OrderBy(x => x.queuepos).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<GameDAO>();
            }
        }
        public async Task CreateTable(GameDAO game)
        {
            try
            {
                _context.Add(game);
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error Creating Table: " + e);
                throw new Exception("Failed to Create New Table");
            }
        }
        public async Task SimpleUpsert(GameDAO game)
        {
            var gameToUpdate = await _context.Games.Where(c => c.id == game.id && c.owner == game.owner).FirstOrDefaultAsync();
            if(gameToUpdate == null)
            {
                await CreateTable(game);
                return;
            }
            else
            {
                _context.Entry(gameToUpdate).CurrentValues.SetValues(game);
                await _context.SaveChangesAsync();
            }
        }
        public async Task SimpleDelete(GameDAO game)
        {
            var gameToRemove = await _context.Games.Where(c => c.id == game.id && c.owner == game.owner).FirstOrDefaultAsync();
            if(gameToRemove == null)
            {
                throw new Exception("No Game Found");
            }
            else
            {
                _context.Games.Remove(gameToRemove);
                await _context.SaveChangesAsync();
            }
        }
        public async Task AddToQueueAsync(GameDAO game)
        {
            try
            {
                // Get the current highest queue position
                var maxQueuePos = await _context.Games
                    .Where(g => g.owner == game.owner && g.queuepos.HasValue)
                    .MaxAsync(g => (int?)g.queuepos) ?? 0;

                // Set the new game's queue position to maxQueuePos + 1
                game.queuepos = maxQueuePos + 1;

                // Update the game in the database
                await SimpleUpsert(game);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error Adding Game to Queue: {e}");
                throw new Exception("Failed to Add Game to Queue");
            }
        }
        public async Task RemoveFromQueueAsync(GameDAO game)
        {
            try
            {
                var gameToUpdate = await _context.Games
                    .Where(c => c.id == game.id && c.owner == game.owner)
                    .FirstOrDefaultAsync();
                if (gameToUpdate == null)
                {
                    throw new Exception("No Game Found to Remove from Queue");
                }
                var removedQueuePos = gameToUpdate.queuepos;
                // Remove the game from the queue
                gameToUpdate.queuepos = null;
                await _context.SaveChangesAsync();
                // Decrement the queue positions of games that were after the removed game
                var gamesToUpdate = await _context.Games
                    .Where(c => c.owner == game.owner && c.queuepos > removedQueuePos)
                    .ToListAsync();
                foreach (var g in gamesToUpdate)
                {
                    g.queuepos -= 1;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error Removing Game from Queue: {e}");
                throw new Exception("Failed to Remove Game from Queue");
            }
        }
    }
}
