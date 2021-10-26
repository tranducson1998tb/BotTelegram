using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleForm.Model
{
    class BotModel
    {
        public string botID { get; set; }

        public List<ActionsModel> actions { get; set; }
    }
}
