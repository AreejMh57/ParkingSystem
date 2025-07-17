using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.DTOs.Log;

namespace Application.External
{
    public interface ILoggingClient
    {
        Task SendLogAsync(CreateLogRequest log);
    }

}

