using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using KanbanAppApi.DataAccess;
using KanbanAppApi.Models;
using KanbanAppApi.Services;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("*");
                          policy.WithHeaders("*");
                          policy.WithMethods("GET", "PUT", "POST", "DELETE");
                      });
});

builder.Services.AddSingleton<IDbConnect, DbConnect>();
builder.Services.AddSingleton<IUsersService, UsersService>();
builder.Services.AddSingleton<IKanbanBoardService, KanbanBoardService>();
builder.Services.AddSingleton<ITasksService, TasksService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("firebase-credentials.json"),
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
