using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CapDemo.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PublishController : Controller
    {
        private readonly ICapPublisher _publisher;
        private readonly IConfiguration _cfg;

        private static ConcurrentDictionary<Guid,TaskCompletionSource<PubMessage>> _tcsDic = new ConcurrentDictionary<Guid, TaskCompletionSource<PubMessage>>();

        public PublishController(ICapPublisher publisher,IConfiguration cfg)
        {
            _publisher = publisher;
            _cfg = cfg;
        }

     
        [HttpGet("{message}")]
        public async Task<PubMessage> Publish(string message)
        {
            var pubMessage = new PubMessage() { Message = message };

            using (var sqlConnection = new SqlConnection(_cfg["ConnectionString"]))
            {
                sqlConnection.Open();
                using (var tran = sqlConnection.BeginTransaction())
                {
                  
                    
                    // your business code
                    
                    await _publisher.PublishAsync("CapDemo.Publish", pubMessage,
                        sqlConnection, "CapDemo.Response",tran);
                    tran.Commit();
                }
            }
            
            var tcs = new TaskCompletionSource<PubMessage>();

            _tcsDic.TryAdd(pubMessage.Id, tcs);

            return await tcs.Task;
        }


        [CapSubscribe("CapDemo.Response")]
        public async Task Respoonse(PubMessage pubMsg)
        {
            await Console.Out.WriteLineAsync(pubMsg.Message);
            
            if (_tcsDic.Remove(pubMsg.Id, out var tcs))
            {
                tcs.SetResult(pubMsg);
                
            }
            
        }
    }

    public class PubMessage
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Message { get; set; }
    }
}