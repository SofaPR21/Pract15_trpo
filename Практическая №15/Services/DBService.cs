using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Практическая__15.Models;

namespace Практическая__15.Services
{
    public class DBService
    {
        private readonly ElectronicsStoreBaseContext _context;

        public ElectronicsStoreBaseContext Context => _context;

        private static DBService? _instance;

        public static DBService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DBService();
                return _instance;
            }
        }

        private DBService()
        {
            _context = new ElectronicsStoreBaseContext();
        }
    }
}
