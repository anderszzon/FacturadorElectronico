using DotNetEnv;
using FacturadorElectronico.Api.Configuraciones;
using FacturadorElectronico.Api.Extensiones;
using FacturadorElectronico.Aplicacion.InyeccionDependencias;

Env.Load();

WebApplicationBuilder constructor = WebApplication.CreateBuilder(args);

ApiConfiguracion configuracion = constructor.Services.InstanciarConfiguracionApi();


constructor.Services.ConfigureDynatraceTrazas(configuracion);
constructor.Logging.ConfigurarDynatraceLogs(configuracion);

constructor.Services.AddCustomCors(configuracion);
constructor.Services.AddEndpointsApiExplorer();
constructor.Services.AddSwaggerGen();
constructor.Services.AgregarCapaAplicacion();

WebApplication app = constructor.Build();

app.ConfigurePipeline();

app.Run();
