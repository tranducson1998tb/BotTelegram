using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleForm.Model
{
    class TransactionModel
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public int Amount { get; set; }
        public string AddressToBuy { get; set; }
        public string AddressToSell { get; set; }

    }
}
