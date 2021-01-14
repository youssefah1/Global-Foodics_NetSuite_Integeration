using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model
{
    public class FoodicsGiftcardtransaction
    {
        public string id { get; set; }
        public decimal amount { get; set; }
        public decimal old_balance { get; set; }
        public decimal new_balance { get; set; }
        public string gift_card_ID { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime deleted_at
        {
            get; set;

        }
    }
}
