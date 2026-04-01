using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.HomeServices
{
    public interface IHomeService
    {
        Task<byte[]> GenerateQrCodeAsync(string qrText);
        Task<byte[]> GenerateEmergencyListPdfAsync(List<emergencyList> emergencyLists);
        Task InsertException(string source, string message, string originatedAt, string stackTrace, string innerExceptionMessage);
    }
}