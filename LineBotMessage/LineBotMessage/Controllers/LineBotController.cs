﻿using LineBotMessage.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LineBotMessage.Controllers
{

    [Route("api/[Controller]")]
    [ApiController]
    public class LineBotController : ControllerBase
    {

        // 貼上 messaging api channel 中的 accessToken & secret
        private readonly string channelAccessToken = "Your channel access token";
        private readonly string channelSecret = "Your channel secret";

        // constructor
        public LineBotController()
        {

        }

        [HttpPost("Webhook")]
        public IActionResult Webhook(WebhookRequestBodyDto body)
        {
            Console.WriteLine(body);
            return Ok();
        }

    }
}