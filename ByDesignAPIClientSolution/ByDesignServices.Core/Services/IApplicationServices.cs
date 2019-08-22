using ByDesignServices.Core.Models.SalesOrders;
using ByDesignServices.Core.Models.StockTransfers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ByDesignServices.Core.Services
{
    public interface IApplicationServices
    {
        Task<Tuple<int, Stream>> UploadSalesOrderAsync(MemoryStream memoryStream);
        Task<Tuple<int, Stream>> UploadSingleSalesOrderAsync(MemoryStream memoryStream);
        Task<Tuple<int, Stream>> UploadPurchaseOrderAsync(MemoryStream memoryStream);
        Task<Tuple<int, Stream>> UploadStockTransfer(MemoryStream memoryStream, StockTransferModel model);
        Task<Tuple<int, Stream>> PostSalesOrder(MemoryStream memoryStream, SalesOrderHeader model);
    }
}
