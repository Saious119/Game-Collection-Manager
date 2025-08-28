using System;
using log4net;
using GameCollectionManager.Shared.Models;
using System.Data;
using System.Net.Security;
using Npgsql;
using Newtonsoft.Json;
using GameCollectionManagerAPI.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace GameCollectionManagerAPI.Services
{
    public class DB_Services : IDB_Service
    {
        List<GameDAO> gameList = new List<GameDAO>();
        private ILog log = LogManager.GetLogger(typeof(Program));
        private readonly IConfiguration _configuration;
        private DataContext _context;
        private IMapper _mapper;

        public DB_Services(IConfiguration configuration, DataContext dataContext, IMapper mapper)
        {
            this._configuration = configuration;
            _context = dataContext;
            _mapper = mapper;
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
    }
}
