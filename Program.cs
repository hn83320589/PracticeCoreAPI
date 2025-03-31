using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using Hangfire;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 將服務加入到容器
builder.Services.AddHttpClient();

// 修正後的 FluentValidation 設定方式
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
    });

// 正確使用 FluentValidation 擴充方法
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 註冊 AutoMapper，掃描包含映射設定檔的組件
builder.Services.AddAutoMapper(typeof(Program));

// 了解更多有關設定 Swagger/OpenAPI，請參考：https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 設定 Serilog
builder.Host.UseSerilog((context, configuration) => 
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day));

var app = builder.Build();

// 設定 HTTP 請求管線
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
