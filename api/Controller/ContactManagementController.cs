using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/[controller]")]
public class ContactManagementController : BaseController
{
    private readonly IStorage storage;
    private readonly PostgreDataAccess _dataAccess;
    private readonly IHubContext<DownloadNotificationHub> _hubContext;

    public ContactManagementController(IStorage storage, IHubContext<DownloadNotificationHub> hubContext)
    {
        this.storage = storage;
        _hubContext = hubContext;

        var connectionString = "Host=localhost;Port=5049;Database=123.my_table;Username=postgres;Password=JackeR22;";
        _dataAccess = new PostgreDataAccess(connectionString);
    }

    [HttpPost("contacts")]
    public IActionResult Create([FromBody] Contact contact)
    {
        bool res = storage.Add(contact);
        if (res)
        {
            return Ok(contact);
        }
        return Conflict("Контакт с указанным ID существует");
    }

    [HttpGet("contacts")]
    public ActionResult<List<Contact>> GetContacts()
    {
        return Ok(storage.GetContacts());
    }

    [HttpDelete("contacts/{id}")]
    public IActionResult DeleteContact(int id)
    {
        bool res = storage.Remove(id);
        if (res) return NoContent();
        return BadRequest("Ошибка id");
    }

    [HttpPut("contacts/{id}")]
    public IActionResult UpdateContact([FromBody] ContactDto contactDto, int id)
    {
        bool res = storage.UpdateContact(contactDto, id);
        if (res) return Ok();
        return Conflict("Контакт с указанным ID не нашёлся");
    }

    [HttpGet("contacts/excel")]
    public async Task<IActionResult> ExportToExcel()
    {
        var contacts = storage.GetContacts();

        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Начинается загрузка файла...");

        await Task.Delay(5000); 

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Contacts");

        worksheet.Cells[1, 1].Value = "ID";
        worksheet.Cells[1, 2].Value = "Name";
        worksheet.Cells[1, 3].Value = "Email";

        for (int i = 0; i < contacts.Count; i++)
        {
            worksheet.Cells[i + 2, 1].Value = contacts[i].Id;
            worksheet.Cells[i + 2, 2].Value = contacts[i].Name;
            worksheet.Cells[i + 2, 3].Value = contacts[i].Email;
        }

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Файл успешно загружен!");

        return File(stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "contacts.xlsx");
    }
}
