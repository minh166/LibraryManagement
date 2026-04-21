using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using WebAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// OData model
var odataBuilder = new ODataConventionModelBuilder();
odataBuilder.EntitySet<Book>("Books");
odataBuilder.EntitySet<BorrowRecord>("BorrowRecords");
odataBuilder.EntitySet<Fine>("Fines");
odataBuilder.EntitySet<Category>("Categories");

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();