using EmailService.Contracts;
using EmailService.Service;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Controllers
{
    [Route("api/[controller]")]
    public class WebhookController : Controller
    {
        private readonly IEmailService _emailService;

        public WebhookController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        // POST api/webhook
        [HttpPost]
        public IActionResult Post([FromBody] Email email)
        {
            //I'm assuming this endpoint is to send email based from the signature
            //The important thing should be to return the proper status code based on
            //the situation. The status codes used right now are placeholders
            if (_emailService.SendEmail(email) == "Success")
            {
                return Ok();
            }
            else {
                return BadRequest();
            }

        }
    }
}