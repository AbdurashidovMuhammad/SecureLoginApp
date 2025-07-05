using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureLoginApp.Application.Models.Users
{
    public class RegisterResponseModel : BaseResponseModel
    {
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
