using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using WebAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// OData model
var odataBuilder = new ODataConventionModelBuilder();
odataBuilder.EntitySet<Book>("Books");
odataBuilder.EntitySet<BorrowRecord>("BorrowRecords");
odataBuilder.EntitySet<Fine>("Fines");
odataBuilder.EntitySet<Category>("Categories");
builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession();
builder.Services.AddControllers()
    .AddOData(options => options
        .Select()
        .Filter()
        .OrderBy()
        .Expand()
        .Count()
        .SetMaxTop(100)
        .AddRouteComponents("odata", odataBuilder.GetEdmModel())
    );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LibrarydbContext>(options =>
    options.UseSqlServer(builder.Configuration
        .GetConnectionString("MyCnn")));

builder.Services.AddSession();

var app = builder.Build();

app.UseSession();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.Run();