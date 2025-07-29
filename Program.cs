using SendGrid;
using SendGrid.Helpers.Mail;

var builder = WebApplication.CreateBuilder(args);

// Allow CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()  // or use WithOrigins("http://localhost:5500") if serving frontend via a dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Configuration.AddJsonFile("appsettings.json");

var app = builder.Build();
// Use CORS
app.UseCors();

app.MapPost("/enquiry", async (Enquiry enquiry, IConfiguration config) =>
{
    var sendGridKey = config["SendGrid:ApiKey"];
    var fromEmail = config["SendGrid:From"];
    var toEmail = config["SendGrid:To"];

    var client = new SendGridClient(sendGridKey);
    var from = new EmailAddress(fromEmail, "Tour Enquiry");
    var to = new EmailAddress(toEmail);
    var subject = $"New Tour Enquiry from {enquiry.Name}";
    var plainTextContent = $"Name: {enquiry.Name}\nEmail: {enquiry.Email}\nPhone: {enquiry.Phone}\nMessage: {enquiry.Message}";


    var htmlContent = $"<strong>Name:</strong> {enquiry.Name}<br/>" +
                  $"<strong>Email:</strong> {enquiry.Email}<br/>" +
                  $"<strong>Phone:</strong> {enquiry.Phone}<br/>" +
                  $"<strong>Message:</strong><br/>{enquiry.Message}";


    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
    var response = await client.SendEmailAsync(msg);

    return response.IsSuccessStatusCode
        ? Results.Ok("Email sent successfully.")
        : Results.StatusCode(500);
});

app.Run();

record Enquiry(string Name, string Email, string Phone, string Message);
