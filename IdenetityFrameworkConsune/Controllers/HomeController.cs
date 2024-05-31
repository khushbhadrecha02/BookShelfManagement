using IdenetityFrameworkConsune.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Web.Helpers;

namespace IdenetityFrameworkConsune.Controllers
{
    public class HomeController : Controller
    {
        private HttpClient client = new HttpClient();
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            // Retrieve token from session
            var token = HttpContext.Session.GetString("token");

            if (string.IsNullOrEmpty(token))
            {
                // Redirect to login if token is not available
                return RedirectToAction("Login");
            }

            // Create a new HttpClient instance with a default bearer token authentication header
            var clientWithToken = new HttpClient();
            clientWithToken.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            foreach (var header in clientWithToken.DefaultRequestHeaders)
            {
                Console.WriteLine($"{header.Key}: {string.Join(",", header.Value)}");
            }

            // Make the request using the HttpClient with the token in the header
            string url = "https://localhost:44360/api/Authorize/protected";
            HttpResponseMessage response = clientWithToken.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(result);
                // Handle successful response
            }
            else
            {
                // Handle unauthorized or other error responses
                // Example: Redirect to an error page
                return RedirectToAction("Error");
            }

            return View();
        }

        public IActionResult Login(LoginModel loginModel)
        {
            string url = "https://localhost:44360/api/Authorize/login";
            string data = JsonConvert.SerializeObject(loginModel);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var tokenObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                string? token = tokenObject?._token;

                // Store token in session
                HttpContext.Session.SetString("token", token);
                Console.WriteLine(HttpContext.Session.GetString("token"));

                    return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
        }
        public IActionResult Register()
        {
            return View("Registration");
        }
        [HttpPost]
        public IActionResult Register(LoginModel loginModel)
        {
            string url = "https://localhost:44360/api/Authorize/register";
            string data = JsonConvert.SerializeObject(loginModel);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var EmailConfirmationUrl = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                string? token = EmailConfirmationUrl.token;
                string? userId  = EmailConfirmationUrl.userId;
                var callbackUrl = Url.Action("ConfirmEmail", "Home", new { _userId = userId, _token = token }, protocol: HttpContext.Request.Scheme);
                SendEmail(loginModel.Email, callbackUrl);
                


                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
            
        }
        public IActionResult SendEmail(string email,string htmlMessage) 
        {
            string url = "https://localhost:44360/api/Authorize/SendEmail";
            var sendEmailRequest = new SendEmailRequest
            {
                Email = email,
                HtmlMessage = htmlMessage
            };
            string data = JsonConvert.SerializeObject(sendEmailRequest);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response =  client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                // Email sent successfully
                var jsonResponse =  response.Content.ReadAsStringAsync().Result;
                // Handle the response as needed
            }
            else
            {
                return NotFound();
            }
            return View("Registration");
        }
        public IActionResult ConfirmEmail(string _userId,string _token)
        {
            string url = "https://localhost:44360/api/Authorize/ConfirmEmail";
            var confirmEmail = new ConfirmEmail
            {
                userID = _userId,
                token = _token
            };
            string data = JsonConvert.SerializeObject(confirmEmail);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                // Email sent successfully
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                return View("EmailConfirmation");
            }
            else
            {
                return View("Index");
            }

        }

    }
}
