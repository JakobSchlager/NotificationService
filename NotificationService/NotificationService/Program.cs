using MailKit;
using MassTransit;
using NotificationService;
using NotificationService.Events;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------- ConfigureServices
// Add services to the container.
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<NotificationService.Services.IMailService, NotificationService.Services.MailService>();

// Masstransit RabbitMQ
var queueSettings = builder.Configuration.GetSection("RabbitMQ:QueueSettings").Get<QueueSettings>();
var rabbitmqHostname = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME"); 
if(rabbitmqHostname != null)
{
    queueSettings.HostName = rabbitmqHostname; 
}
Console.WriteLine($"queueSettings.HostName = {queueSettings.HostName}");

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PDFCreatedEventConsumer>().Endpoint(x => x.Name = "PDFCreated_queue"); 

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(queueSettings.HostName, queueSettings.VirtualHost, h =>
        {
            h.Username(queueSettings.UserName);
            h.Password(queueSettings.Password);
        });
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddMassTransitHostedService();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// -------------------------------------------- ConfigureServices END

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
