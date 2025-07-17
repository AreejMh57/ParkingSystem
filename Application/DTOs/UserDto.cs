using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UserDto
    {
        public String UserId { get; set; }
       
        public string Email { get; set; }

       
        public string PhoneNumber { get; set; }

        
        public string Password { get; set; }
        
        public  string Token   {get; set; }
    
        public Guid WalletId { get; set; }
    }
}
