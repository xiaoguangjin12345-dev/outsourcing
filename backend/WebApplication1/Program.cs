using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services;
using OutsourcingApplication.Services.Interfaces;
using Microsoft.OpenApi.Models;
using OutsourcingApplication.Services.Utils;

// 禁用.NET默认的Claims映射
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// 读取配置
var jwtSettings = builder.Configuration.GetSection("Jwt");
var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

// 身份验证
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = securityKey,

        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
        NameClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/name",

        ClockSkew = TimeSpan.Zero
    };
});

// 开启权限控制
builder.Services.AddAuthorization();


// 获取在appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 注册DbContext (这里取消了失败自动重试，配合业务逻辑)
builder.Services.AddDbContext<OutsourcingDbContext>(options =>
    options.UseSqlServer(connectionString)); 

// 注册控制器服务
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// 注册具体服务（绑定接口和实现类）
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IPerformanceService, PerformanceService>();
builder.Services.AddScoped<IWorkLogService, WorkLogService>();
builder.Services.AddScoped<INoticeService, NoticeService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IFileService, FileService>();


// 配置Swagger/OpenAPI
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "软件外包项目管理系统", Version = "v1" });

    // 安全定义
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "请在下方输入: Bearer {你的Token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // 所有的接口默认都需要该安全要求
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// 跨域设置
builder.Services.AddCors(options => {
    options.AddPolicy("Any", p =>
        p.WithOrigins("http://localhost:5174", "http://localhost:5173") // 明确允许前端的两个端口
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()); // 涉及Cookie 认证
});


var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP请求重定向到HTTPS，开发阶段注释该句，投入生产环境后需要取消注释
// app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("Any");


app.UseAuthentication(); // 检查Token
app.UseAuthorization();  // 检查Role

app.MapControllers();

app.Run();
