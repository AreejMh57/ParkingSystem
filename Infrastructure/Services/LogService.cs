using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IServices;
using Application.DTOs.Log;
using Application.External;
using Domain.Entities;
using Domain.IRepositories;
using AutoMapper;


namespace Infrastructure.Services
{
    public class LogService : ILogService
    {
        private readonly ILoggingClient _loggingClient;

        public LogService(ILoggingClient loggingClient)
        {
            _loggingClient = loggingClient;
        }

        public async Task LogInfoAsync(string message)
        {
            try
            {
                await _loggingClient.SendLogAsync(new CreateLogRequest
                {
                    Message = message,
                    Level = "Info",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to send Info log: {ex.Message}");
            }
        }

        public async Task LogWarningAsync(string message)
        {
            try
            {
                await _loggingClient.SendLogAsync(new CreateLogRequest
                {
                    Message = message,
                    Level = "Warning",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to send Warning log: {ex.Message}");
            }
        }

        public async Task LogErrorAsync(string message)
        {
            try
            {
                await _loggingClient.SendLogAsync(new CreateLogRequest
                {
                    Message = message,
                    Level = "Error",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to send Error log: {ex.Message}");
            }
        }

    }
}
