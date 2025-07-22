using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.IServices
{
    public interface ISensorService
    {
      
        // Creates a new sensor record.
        
        Task<SensorDto> CreateSensorAsync(CreateSensorDto dto);

        // Retrieves all sensor records in the system.
      
        Task<IEnumerable<SensorDto>> GetAllSensorsAsync();

        
        // Retrieves a single sensor record by its ID.
     
        Task<SensorDto?> GetSensorByIdAsync(Guid sensorId);

    
        // Retrieves all sensors for a specific garage.

        Task<IEnumerable<SensorDto>> GetSensorsByGarageIdAsync(Guid garageId);


        // Updates the status (active/inactive) or other details of an existing sensor.
     
        Task<SensorDto> UpdateSensorAsync(Guid sensorId, UpdateSensorDto dto);

      
        // Deletes a sensor record from the system. (Admin-level operation)
     
        Task<bool> DeleteSensorAsync(Guid sensorId);
    }
}
