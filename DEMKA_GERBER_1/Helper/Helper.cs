using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DEMKA_GERBER_1.MODELS;

namespace DEMKA_GERBER_1.Helper
{
    public class Helper
    {
        private static DEMKA_GERBER _context;
        public static DEMKA_GERBER GetContext()
        {
            if (_context == null)
                _context = new DEMKA_GERBER();
            return _context;
        }
    }
}
