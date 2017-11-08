using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;

namespace CapDemo.Handler.Controllers
{
    public class SubscribeController : Controller
    {
        [CapSubscribe("CapDemo.Publish")]
        public async Task<SubMessage> Subscribe(SubMessage subMessage)
        {
            await Console.Out.WriteLineAsync(subMessage.Message);

            return subMessage;
        }
    }


    public class SubMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Message { get; set; }
    }
}