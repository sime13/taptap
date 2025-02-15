using PdfiumViewer;

var builder = WebApplication.CreateBuilder(args);

// Adicionar servi�os MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Caminho da dll
var pdfiumPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/lib/pdfium");

// Garantir que a DLL existe antes de definir a vari�vel de ambiente
if (Directory.Exists(pdfiumPath))
{
    Environment.SetEnvironmentVariable("PDFIUM_DLL_PATH", pdfiumPath);
}

// Configura��o do pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
